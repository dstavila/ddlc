[Select]
enum ETesto
{
	kUnknown = 0,
	kTwo,
	kThree,
}

[Command] void ToolSelected(uint ToolId, float foo);
[Command] void MoveTool();


namespace Hey
{
	[Select]
	enum EHeyTesto
	{
		kUnknown = 0,
		kOne,
	}
}