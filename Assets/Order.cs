using System;

[Serializable]
public class Order
{
	public string Type;
	public object Target;
	public Func<bool> IsDone;

	public class Types
	{
		public const string Attack = "attack";
		public const string Move = "move";
	}
}