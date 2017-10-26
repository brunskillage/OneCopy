using System;
using OneCopy2017.DataObjects;

namespace OneCopy2017.Services
{
    public class EventService
    {
        public event EventHandler<OneCopyEvent> OnTalk = (sender, args) => { };

        private void RaiseTalk(object sender, OneCopyEvent args)
        {
            var evt = OnTalk;
            evt?.Invoke(sender, args);
        }

        public void Talk(string words)
        {
            if (string.IsNullOrWhiteSpace(words))
                return;

            RaiseTalk(null, new OneCopyEvent {Message = words});
        }
    }
}