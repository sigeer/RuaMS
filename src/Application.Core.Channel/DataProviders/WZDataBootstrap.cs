namespace Application.Core.Channel.DataProviders
{
    public abstract class WZDataBootstrap
    {
        bool isLoading = false;
        bool loaded = false;
        public void LoadData()
        {
            if (isLoading)
                return;

            if (loaded)
                return;

            isLoading = true;
            LoadDataInternal();
            isLoading = false;
            loaded = true;
        }

        protected abstract void LoadDataInternal();
    }
}
