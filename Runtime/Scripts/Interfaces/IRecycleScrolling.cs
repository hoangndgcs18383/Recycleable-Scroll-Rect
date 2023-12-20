namespace Hzeff.UI
{
    public interface IRecycleScrollProvider
    {
        int GetItemCount();
        void SetData(IScrollData cell, int index);
    }
}