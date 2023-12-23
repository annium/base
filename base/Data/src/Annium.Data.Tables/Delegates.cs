namespace Annium.Data.Tables;

public delegate int GetKey<in T>(T source);
public delegate bool HasChanged<in T>(T source, T value);
public delegate void Update<in T>(T source, T value);
