// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/MountPointsBlit" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MaskTex ("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
    #pragma glsl
	
	sampler2D	_MainTex;
	sampler2D   _MaskTex;
	
	struct VertexStruct 
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	//Common Vertex Shader
	VertexStruct vert( appdata_img rInput )
	{
		VertexStruct lOutput;
		lOutput.pos = UnityObjectToClipPos (rInput.vertex);
		lOutput.uv = rInput.texcoord.xy;
		return lOutput;
	} 
	
	half4 frag(VertexStruct rInput) : COLOR
	{		
		float4 lBaseColor = tex2D(_MainTex, rInput.uv);
		
		float4 lMaskColor = tex2D(_MaskTex, rInput.uv);
		lBaseColor.a = lBaseColor.a * lMaskColor.r;
		
		return lBaseColor;
	}

	ENDCG	
	 
	Subshader 
	{
		ZTest Off
		Cull Off
		ZWrite Off
		Lighting Off
		Fog { Mode off }

		//Pass 0 Mask
		Pass 
		{
			Name "Mask"
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		} 
	}
}
