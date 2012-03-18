using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Drawing;

namespace GameNick_net
{
    [ServiceContract]
    public interface IGameNickService
    {

        [OperationContract]
        User AddGameNick(User accessToken, GameNick gameNick);
        [OperationContract]
        User Authenticate(string facebookAccessToken);

        [OperationContract]
        long CreateEvent(User accessToken, GamingEvent gamingEvent, List<User> inviteList);

        [OperationContract]
        List<Status> GetStatus(Guid accessToken, int userId);

        [OperationContract(Name = "GetEventsForGame")]
        List<GamingEvent> GetEvents(User accessToken, Game game);

        [OperationContract(Name = "GetEventsForUser")]
        List<GamingEvent> GetEvents(User accessToken, User user);

        [OperationContract]
        List<User> GetFriends(User user);

        [OperationContract]
        User UpdateStatus(User accessToken, int gameNickId, string status);
        
        [OperationContract]
        List<GameNick> GetGameNicks(Guid accessToken, int userId);

        [OperationContract]
        List<Game> GetGames(Guid accessToken, int platformId);

        [OperationContract]
        List<Platform> GetPlatforms(Guid accessToken);

        [OperationContract]
        User RemoveGameNick(User accessToken, GameNick gameNick);

        [OperationContract]
        List<User> GetFriendsWithGame(Guid accessToken, int gameId);

        [OperationContract]
        List<Status> GetStatusWithGame(Guid accessToken, int gameId);
    }
    public struct GamingEvent
    {
        public Event Event;
        public string Name;
        public string Description;
        public DateTime StartTime;
        public DateTime EndTime;
        public string Location;
        public Uri EventPage { get { return new Uri("http://facebook.com/events/" + Event.FacebookID);} }
    }
}
