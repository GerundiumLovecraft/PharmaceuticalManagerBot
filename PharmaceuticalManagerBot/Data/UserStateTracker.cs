using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmaceuticalManagerBot.Data
{
    public interface IUserStateTracker
    {
        void SetState(long chatId, string state);
        string GetState(long chatId);
        void RemoveState(long chatId);
    }

    public class UserStateTracker : IUserStateTracker
    {
        private readonly ConcurrentDictionary<long, string> _userStates = new();

        public void SetState(long chatId, string state) => _userStates.AddOrUpdate(chatId, state, (_, _) => state);
        public string GetState(long chatId) => _userStates.TryGetValue(chatId, out string state) ? state : null;
        public void RemoveState(long chatId) => _userStates.TryRemove(chatId, out _);
    }
}
