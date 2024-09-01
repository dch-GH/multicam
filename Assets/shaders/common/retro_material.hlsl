class RetroMaterial
{
	float3 Albedo;
	float3 Emission;			
	float TintMask;				//Tint objects
	float Opacity;	

	//Lighting			
	float3 Normal;				//World Space Normal
	float EnvironmentMapMask;	//Reveal envmap specular highlights
	float3 EnvironmentMap; 		//Artist authored environment map cube to sample reflections from, think HL1 revolver chrome
	float3 WorldPosition;

	static RetroMaterial Init()
	{
		Albedo = float3(0.5, 0.5, 0.5);
		Emission = float3(0.0, 0.0, 0.0);
		Normal = float3(0.0, 0.0, 1.0);
		WorldPosition = float3(0.0, 0.0, 0.0);
		EnvironmentMapMask = 0.0;
		EnvironmentMap = float3(0.0, 0.0, 0.0);
		TintMask = 1.0;
		Opacity = 1.0;
	}
}