namespace DDL
{
	[Select]
	enum ETesto
	{
		kUnknown = 0,
		kTesto,	
	}
	struct A
	{
		struct B
		{
			struct C
			{
				int val;
			}

			int val2;
			C c;
		}
		int val3;
		B b;
	}
}
