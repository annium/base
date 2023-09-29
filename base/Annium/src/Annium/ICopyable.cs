namespace Annium;

public interface ICopyable<out T>
{
    T Copy();
}