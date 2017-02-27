namespace PSD.PSDGamepkg.Artiad
{
    public abstract class NGT
    {
        public abstract bool Legal();

        public abstract string ToMessage();

        public void Hotel(XI XI)
        {
            if (Legal())
                XI.RaiseGMessage(ToMessage());
        }
    }
}
