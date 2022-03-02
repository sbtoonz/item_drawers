using System.Reflection;
public static class CommonUtils
{ 
	public static T Clone<T>(T obj)
	{
		return (T)obj.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(obj, null);
	}
}
