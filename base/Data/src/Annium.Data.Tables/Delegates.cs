namespace Annium.Data.Tables;

public delegate int GetKey<in T>(T source);
public delegate bool HasChanged<in TSource, in TUpdate>(TSource source, TUpdate value);
public delegate void Update<in TSsource, in TUpdate>(TSsource source, TUpdate value);
