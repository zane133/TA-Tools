Shader "Hidden/TextureCombiner"
{
	Properties
	{
		_MainTex ("source", 2D) = "black" {}

		_TexR ("R", 2D) = "black" {}
		_TexG ("G", 2D) = "black" {}
		_TexB ("B", 2D) = "black" {}
		_TexA ("A", 2D) = "white" {}

		_SrcR ("R Source", Float) = 0
		_SrcG ("G Source", Float) = 0
		_SrcB ("B Source", Float) = 0
		_SrcA ("A Source", Float) = 0
	}
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		//PASS 0: Combine
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _TexR;
			sampler2D _TexG;
			sampler2D _TexB;
			sampler2D _TexA;

			float _SrcR;
			float _SrcG;
			float _SrcB;
			float _SrcA;

			inline float GetChannel(in float4 source, in float sourceChannel)
			{
				float r = 1;
				switch(sourceChannel)
				{
					case 0: r = source.r; break;
					case 1: r = source.g; break;
					case 2: r = source.b; break;
					case 3: r = source.a; break;
					case 4: r = Luminance(source.rgb); break;
				}
				return r;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float4 r4 = tex2D(_TexR, i.uv);
				float4 g4 = tex2D(_TexG, i.uv);
				float4 b4 = tex2D(_TexB, i.uv);
				float4 a4 = tex2D(_TexA, i.uv);

				float r = GetChannel(r4, _SrcR);
				float g = GetChannel(g4, _SrcG);
				float b = GetChannel(b4, _SrcB);
				float a = GetChannel(a4, _SrcA);

				return float4(r,g,b,a);
			}
			ENDCG
		}

		//PASS 1: Get Alpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			sampler2D _MainTex;
			fixed4 frag (v2f i) : SV_Target
			{
				float4 main = tex2D(_MainTex, i.uv);
				return main.aaaa;
			}
			ENDCG
		}
	}
}
