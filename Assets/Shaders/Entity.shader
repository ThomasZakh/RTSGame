﻿Shader "Custom/Entity" {
	Properties{
		_MainTex("Diffuse Map", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Opacity("Opacity", Range(0, 1)) = 1
		//[MaterialToggle]
		//_ComicShading("Use Comic Shading", Float) = 1
		[MaterialToggle]
		_StripedDissolve("Use Striped Dissolve", Float) = 0

		_SSSTex("SSS Map", 2D) = "black" {}
		_SSSStrength("SSS Strength", Range(0, 4)) = 0.5
		_SSSColor("SSS Color", Color) = (1,1,1,1)
		
		_AOTex("AO Map", 2D) = "white" {}
		//_AmbientMult("Ambient Multiplier", Range(0,4)) = 1
		//_AmbientFallL("Ambient Falloff Light", Range(0,1)) = 0.5
		//_AmbientFallD("Ambient Falloff Dark", Range(0,1)) = 0
	}
	SubShader{
	Tags{ "RenderType" = "Opaque" }
	CGPROGRAM
	#pragma surface surf Custom noambient
	//#pragma target 3.0
	//#pragma debug
	sampler2D _MainTex;
	fixed4 _Color;
	half _Opacity;
	half _ComicShading;
	half _StripedDissolve;

	sampler2D _SSSTex;
	fixed4 _SSSColor;
	half _SSSStrength;

	sampler2D _AOTex;
	//half _AmbientMult = 1;
	//half _AmbientFallL = 0.75f;
	//half _AmbientFallD = 0f;

	float stripe(half2 co, float rate)
	{
		float a = 1;
		float b = 1;
		float aspectRatio = _ScreenParams.x / _ScreenParams.y;
		float dxy = dot(half2(co.x, co.y / aspectRatio), half2(a, b));
		return frac(dxy * rate);
	}

	float checker(half2 co, float rate)
	{
		float a = 1;
		float b = 1;
		float aspectRatio = _ScreenParams.x / _ScreenParams.y;
		float dx = dot(co.x, half2(a, b));
		float dy = dot(co.y / aspectRatio, half2(a, b));
		rate *= 5;
		float numberRaw = sin(dx * rate) + cos(dy * rate);
		return clamp(((numberRaw + 4) * 0.125), 0, 1);
	}

	float rand(half2 co)
	{
		float a = 12.9898;
		float b = 78.233;
		float c = 43758.5453;
		float dt = dot(co.xy, half2(a, b));
		float sn = fmod(dt, 3.14);
		return frac(sin(sn) * c) * 1;
	}

	struct SurfaceOutputCustom
	{
		fixed3 Albedo;
		fixed3 AmbientOcclusion;
		fixed3 SSS;
		fixed3 Normal;
		fixed3 Emission;
		half Specular;
		fixed Gloss;
		fixed Alpha;

		float2 screenUV;
		float2 mainUV;
		float3 viewDir;
		//half lightCounter;
	};

	struct Input {
		float2 uv_MainTex;
		//float2 uv_AOTex;
		//float2 uv_SSSTex;
		float4 screenPos;
		float3 worldPos;
		float3 viewDir;
		//float4 _ScreenParams;
	};

	float4 screenPos;

	half3 ambientLight(SurfaceOutputCustom s, half NdotL)
	{
		// Ambient light settings
		half _AmbientMultL = 0.9f; // How bright is the AL in direct light?
		half _AmbientMultD = 1.1f; // How bright is the AL in darkness?
		half _AmbientFallL = 0.75f; // How much diffuse color is retained based on how head-on the direct light is (in the light)?
		half _AmbientFallD = 0.0f; // How much diffuse color is retained based on how head-on the direct light is (in the dark)?
		half _EnvMult = 0.6f; // How much does the environment color affect the color of ambient lighting? How much should the SSS color factor in instead?

		// Sample color of the environment
		half height = clamp(dot(s.Normal, float3(0, 1, 0)) + 1, 0, 1) * 0.5 + clamp(dot(s.Normal, float3(0, 1, 0)) * 0.5, 0, 1);
		half3 env = height > 0.5 ? lerp(unity_AmbientEquator, unity_AmbientSky, abs(height - 0.5) * 2) : lerp(unity_AmbientEquator, unity_AmbientGround, abs(height - 0.5) * 2);

		// Environment color in the dark should bias towards the sky (highest) color
		half3 envLightL = (env);
		half eldA = 0.7;
		half3 envLightD = (env * eldA + unity_AmbientSky * (1 - eldA)) ;

		// Environment color is mixed with the SSS color
		half3 baseLight = _SSSColor;

		// Light/dark affects color choice
		half directionalMult = (clamp(1 - NdotL * _AmbientFallL, 0, 1) + clamp(1 - -NdotL * _AmbientFallD, 0, 1) - 1);
		half3 envLight = lerp(envLightL, envLightD, directionalMult);

		half3 ambientLight = (envLight * _EnvMult + baseLight * (1 - _EnvMult));
		half ambientMult = lerp(_AmbientMultL, _AmbientMultD, directionalMult); // TODO: Should this be reversed?

		// Final color
		ambientLight *= directionalMult;

		//ambientLight = 1;
		return clamp(ambientLight * ambientMult * 0.5, 0, ambientMult);
	}

	half3 mod(half3 a, half3 b)
	{
		return clamp(2 * a - ceil(a * b) / b, 0, 1);
	}

	half dissolveLight(half light, float2 uv, half mix)
	{
		half dissolveLight = (1 - light);
		//half dissolveNoise = rand(s.screenUV);
		half lightTile = 100;
		half lightStripe = 0 ? stripe(uv, lightTile) : checker(uv, lightTile);
		half lit = dissolveLight + ceil(clamp(lightStripe - (1 - dissolveLight), 0, 1)) + 0.0001;
		lit = (1 - lit);
		half litFactor = mix;
		return (clamp(lit, 0, 1) * litFactor + light * (1 - litFactor));
	}

	half dissolveAlpha(float2 uv)
	{
		half dissolveAlpha = (1 - _Opacity);
		half alphaTile = 100;
		half dissolveNoise = _StripedDissolve ? stripe(uv, alphaTile) : rand(uv);
		half dissolve = dissolveAlpha + ceil(clamp(dissolveNoise - (1 - dissolveAlpha), 0, 1)) + 0.0001;
		dissolve = (1 - dissolve);
		return dissolve;
	}

	half4 LightingCustom(SurfaceOutputCustom s, half3 lightDir, half atten) {
		half NdotL = dot(s.Normal, lightDir);
		half NdotV = dot(s.Normal, s.viewDir);
		half LdotV = dot(lightDir, s.viewDir);
		half4 c;

		//_ComicShading = 1;

		// SSS is calculated based on how close the view angle is to the light angle
		half sssDot = LdotV;
		half sssRange = (clamp(sssDot + 1, 0, 1) * 0.5 + clamp(sssDot * 0.677, 0, 1));
		half sssMult = 2 * clamp(1 - (sssRange + 0.5), 0, 1) + 0 * clamp(sssRange - 0.5, 0, 1);
		half3 sss = sssMult * s.SSS * _SSSColor * _LightColor0.rgb * _SSSStrength;

		half light = NdotL * atten;
		half comicMix = 0.5f;
		if (_ComicShading)
			light = dissolveLight(light, s.screenUV, comicMix);

		//half3 shade = _LightColor0.rgb * clamp((1 / (1 + lightMod)) * (light + lightMod), 0, 1);
		half3 shade = _LightColor0.rgb * clamp(light, 0, 1);

		//c.rgb = clamp((s.AmbientOcclusion), 0, 1) * (shade * s.Albedo + ambientLight(s, light) * (s.Albedo + 1) * 0.5) + sss;

		half ambAlbedoMix = 0.6f;
		// Ambient light is mixed in with the albedo
		half3 amb = ambientLight(s, light) * (s.Albedo * ambAlbedoMix + 1 * (1 - ambAlbedoMix));

		half occlusion = s.AmbientOcclusion;
		if (_ComicShading)
			occlusion = dissolveLight(occlusion, s.screenUV, comicMix);
		c.rgb = clamp((occlusion), 0, 1) * (shade * s.Albedo + amb) + sss;
		//c.rgb = round((checker(s.mainUV, 100) + clamp(NdotL * atten, 0, 1)) / 2);
		//c.rgb = lightStripe;

		/*
		half missingAlpha = ((1 - s.Alpha));
		half dissolve = rand(s.screenUV); // Random noise
		clip(1 - (missingAlpha * clamp(dissolve, 0, 1) + missingAlpha));
		*/
		
		c.a = dissolveAlpha(s.screenUV);

		clip(c.a);
		//c.a = clamp(c.a, 0, 1);
		
		return c;
	}





	void surf(Input IN, inout SurfaceOutputCustom o) {
		float2 uv = IN.uv_MainTex;
		o.Albedo = tex2D(_MainTex, uv).rgb * _Color;
		o.Alpha = _Color.a;
		//uv = IN.uv_AOTex;
		o.AmbientOcclusion = 1;
		o.AmbientOcclusion = tex2D(_AOTex, uv).rgb;
		o.SSS = tex2D(_SSSTex, uv).rgb;

		o.viewDir = IN.viewDir;

		float4 screenParams = _ScreenParams;
		float2 screenUV = IN.screenPos.xy / (IN.screenPos.w == 0 ? 1 : IN.screenPos.w);
		o.screenUV = screenUV;
		o.screenUV.x = floor(screenUV.x * screenParams.x) / screenParams.x;
		o.screenUV.y = floor(screenUV.y * screenParams.y) / screenParams.y;

		o.mainUV = uv;
	}
	ENDCG
		}
			Fallback "Diffuse"
}