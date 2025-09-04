Shader /*ase_name*/ "ASETemplateShaders/DefaultUnlit" /*end*/
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Alpha("alpha",Float) = 1
        /*ase_props*/
    }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
        LOD 100
	Cull Off

	/*ase_pass*/
        Pass
        {
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
	    #pragma target 3.0 
	    #pragma vertex vert
	    #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            
            /*ase_pragma*/

	    struct appdata
	    {
		 float4 vertex : POSITION;
		 float4 texcoord : TEXCOORD0;
		 float4 texcoord1 : TEXCOORD1;
		 /*ase_vdata:p=p;uv0=tc0.xy;uv1=tc1.xy*/
                 UNITY_VERTEX_INPUT_INSTANCE_ID
	    };
			
	    struct v2f
	    {
		 float4 vertex : SV_POSITION;
		 float4 texcoord : TEXCOORD0;
		 /*ase_interp(1,7):sp=sp.xyzw;uv0=tc0.xy;uv1=tc0.zw*/
                 UNITY_VERTEX_INPUT_INSTANCE_ID
		 UNITY_VERTEX_OUTPUT_STEREO
	    };          

            TEXTURE2D(_MainTex); 
	    SAMPLER(sampler_MainTex);
	    CBUFFER_START(UnityPerMaterial)
		 half4 _Color;
	    half _Alpha;
	    CBUFFER_END
	    /*ase_globals*/

	    v2f vert ( appdata v /*ase_vert_input*/)
	    {
		 v2f o;
		 UNITY_SETUP_INSTANCE_ID(v);
		 UNITY_TRANSFER_INSTANCE_ID(v, o);
		 UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		 o.texcoord.xy = v.texcoord.xy;
		 o.texcoord.zw = v.texcoord1.xy;
				
		 // ase common template code
		 /*ase_vert_code:v=appdata;o=v2f*/
				
		 o.vertex.xyz += /*ase_vert_out:Local Vertex;Float3*/ float3(0,0,0) /*end*/;
		 VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
		 o.vertex = vertexInput.positionCS;
		 return o;
	    }
			
	    half4 frag (v2f i /*ase_frag_input*/) : SV_Target
	    {
		 UNITY_SETUP_INSTANCE_ID( i );
		 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );
		 half4 myColorVar;
		 // ase common template code
		 /*ase_frag_code:i=v2f*/
				
		 myColorVar = /*ase_frag_out:Frag Color;Float4*/half4(1,0,0,1)/*end*/;
	        float alpha = /*ase_frag_out:Alpha;Float;3;-1;_Alpha*/1/*end*/;
		 return (myColorVar,alpha);
	    }
            ENDHLSL
        }
    }
}