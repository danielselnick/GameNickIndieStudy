using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceModel.Activation;
using System.ServiceModel.Security;
using Facebook;

namespace GameNick_net
{
    [System.Runtime.InteropServices.GuidAttribute("3260DC5A-69C5-4737-94D5-382D3EEEBA9C")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class GameNickService : IGameNickService
    {
        #region GameNickServiceImplementation

        public User AddGameNick(User accessToken, GameNick gameNick) 
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                if(     (gameNick.UserID == accessToken.ID)
                    &&  (gameNick.GameID > 0)
                    &&  (gameNick.Name != string.Empty))
                {
                    entities.AddToGameNicks(gameNick);
                    entities.SaveChanges();
                    return GetMe(entities, accessToken.ID);
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
            throw new SecurityAccessDeniedException();
        }
        public User Authenticate(string facebookAccessToken)
        {
            GameNickEntities entities = new GameNickEntities();
            FacebookClient fbclient = new FacebookClient(facebookAccessToken);
            dynamic me = fbclient.Get("me");
            long myId = (long)Convert.ToUInt64(me.id.ToString());
            var exists = from users in entities.Users
                         where users.FacebookID == myId
                         select users;
            if (exists.Count() > 0)
            {
                exists.First().Name = me.name;
                exists.First().FacebookAccessToken = facebookAccessToken;
                exists.First().GameNickAccessToken = Guid.NewGuid();
                entities.SaveChanges();
                User _me = exists.First();
                _me.Status.Load();
                _me.GameNicks.Load();
                return _me;
            }
// ReSharper disable RedundantIfElseBlock
            else
// ReSharper restore RedundantIfElseBlock
            {
                User user = User.CreateUser(myId, 0);
                user.Name = me.name;
                // check that the ID is updated to a new unique value when it's added
                entities.AddToUsers(user);
                entities.SaveChanges();
                // todo: update the userID before returing it
                return entities.Users.First(u => u.FacebookID == myId);//user;
            }
        }

        public long CreateEvent(User accessToken, GamingEvent eventInfo, List<User> inviteList)
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                FacebookClient facebookClient = new FacebookClient(accessToken.FacebookAccessToken);
                Dictionary<string, object> createEventParameters = new Dictionary<string, object>();
                createEventParameters.Add("name", eventInfo.Name);
                createEventParameters.Add("start_time", eventInfo.StartTime.ToString());
                createEventParameters.Add("end_time", eventInfo.EndTime.ToString());
                createEventParameters.Add("owner", accessToken.FacebookID.ToString());
                createEventParameters.Add("privacy", "OPEN");
                createEventParameters.Add("description", eventInfo.Description);
                try
                {
                    dynamic result = facebookClient.Post("me/events", createEventParameters);

                    if (result != null)
                    {
                        long eventId = (long) Convert.ToInt64(result["id"].ToString());
                        if (inviteList != null) 
                            if(inviteList.Count > 0)
                        {
                            string inviteString = "/invited?users=";
                            for (int i = 0; i < inviteList.Count(); i++)
                            {
                                inviteString += inviteList[i].FacebookID.ToString();
                                if (i < inviteList.Count() - 1)
                                    // don't add a , at the end
                                    inviteString += ",";
                            }

                            dynamic inviteResult =
                                 facebookClient.Post(eventId.ToString() + inviteString);
                        }
                        // add event to database
                        Event eEvent = Event.CreateEvent(0, eventInfo.Event.GameID, eventId);
                        entities.AddToEvents(eEvent);
                        entities.SaveChanges();
                        return eventId;
                    }
                }
                catch (Exception e)
                {
                    
                    throw;
                }

                throw new Exception();
            }
            throw new SecurityAccessDeniedException();
        }

        public List<GamingEvent> GetEvents(User accessToken, Game game)
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                List<GamingEvent> events = new List<GamingEvent>();
                var result = entities.Events.Where(eEvent => eEvent.GameID == game.ID).ToList();
                foreach (Event eEvent in result)
                {
                    try
                    {
                        FacebookClient facebookClient = new FacebookClient(accessToken.FacebookAccessToken);
                        dynamic fbEvent = facebookClient.Get(eEvent.FacebookID.ToString());
                        GamingEvent nEvent = new GamingEvent();
                        nEvent.Event = eEvent;
                        if(fbEvent.description != null)
                            nEvent.Description = fbEvent.description.ToString();
                        if(fbEvent.start_time != null)
                            nEvent.StartTime = Convert.ToDateTime(fbEvent.start_time.ToString());
                        if(fbEvent.end_time != null)
                            nEvent.EndTime = Convert.ToDateTime(fbEvent.end_time.ToString());
                        if(fbEvent.location != null) 
                            nEvent.Location = fbEvent.location.ToString();
                        if(fbEvent.name != null)
                            nEvent.Name = fbEvent.name.ToString();
                        events.Add(nEvent);
                    }
                    catch (Exception e)
                    {
                        // todo: log error here
                        
                    }
                }
                if (events.Count == 0)
                {
                    events.Add(new GamingEvent() {Name = "No events here"});
                }
                return events;
            }            
            throw new SecurityAccessDeniedException();
        }

        public List<GamingEvent> GetEvents(User accessToken, User user)
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                // Get users Facebook events
                FacebookClient facebookClient = new FacebookClient(accessToken.FacebookAccessToken);
                // Check to see which of these events are also GameNick events
                dynamic result = facebookClient.Get(user.FacebookID + "/events");
                List<GamingEvent> gamingEvents = new List<GamingEvent>();
                if (result.data == null)
                    return new List<GamingEvent>() {new GamingEvent() {Name = "No Events"}};
                foreach (dynamic o in result.data)
                {
                    DateTime startTime = Convert.ToDateTime(o.start_time);
                    // If the event date is today or later than today
                    if (startTime >= DateTime.Today)
                    {
                        // If the event is in our database
                        long facebookId = Convert.ToInt64((o.id.ToString()));
                        var e = entities.Events.Where(evnt => evnt.FacebookID == facebookId);
                        if (e.Count() > 0)
                        {
                            // Build the event info to send back to the user
                            Event eEvent = e.First();
                            dynamic facebookEvent = facebookClient.Get(eEvent.FacebookID.ToString());
                            GamingEvent gamingEvent = new GamingEvent()
                                                          {
                                                              Description = facebookEvent.description,
                                                              EndTime = facebookEvent.end_time,
                                                              StartTime = facebookEvent.start_time,
                                                              Event = eEvent,
                                                              Name = facebookEvent.name,

                                                          };
                            gamingEvents.Add(gamingEvent);
                        }
                    }
                }
                return gamingEvents;
            }
            throw new SecurityAccessDeniedException();
        }

        public List<User> GetFriends(User user)
        {
            // Get the friends of a user
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(user, entities))
            {
                try
                {
                List<User> friends = new List<User>();
                // Get the Facebook friends of the user
                FacebookClient facebookClient = new FacebookClient(user.FacebookAccessToken);
                dynamic facebookFriends = facebookClient.Get("me/friends");
                JsonArray fArray = (JsonArray) facebookFriends.data;
                // Check if each Facebook friend is in the database
                foreach (JsonObject friend in fArray)
                {
                    long id = (long) Convert.ToUInt64(friend[1]);
                    var isUser = entities.Users.Where(userid => userid.FacebookID == id);
                    if (isUser.Count() > 0)
                    {
                        User myFriend = isUser.First();
                        myFriend.FacebookAccessToken = string.Empty;
                        myFriend.GameNickAccessToken = null;
                        myFriend.Name = friend["name"].ToString();
                        friends.Add(myFriend);
                    }
                }
                // return the list
                return friends;
                }
                catch (Exception e)
                {
                    return new List<User>() {new User() {Name = e.Message, FacebookAccessToken = e.InnerException.ToString()}};
                    throw;
                }
                
            }
            throw new SecurityAccessDeniedException();
        }

        public List<GameNick> GetGameNicks(Guid accessToken, int userId)
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                var result = entities.GameNicks.Where(gn => gn.UserID == userId);
                foreach (GameNick gameNick in result)
                {
                    gameNick.GameReference.Load();
                    gameNick.Game.PlatformReference.Load();
                }
                return result.ToList();
            }            
            throw new SecurityAccessDeniedException();
        }

        public List<User> GetFriendsWithGame(Guid accessToken, int gameId)
        {
            // Authenticated?
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                User me = GetMe(entities, accessToken);
                // Get my friends
                var myFriends = this.GetFriends(me);
                List<User> friendsWGame = new List<User>();
                // Check to see if any of my friends have this game
                foreach (User friend in myFriends)
                {
                    var hasGame = entities.GameNicks.Where(g => ((g.GameID == gameId) && (g.UserID == friend.ID)));
                    if (hasGame.Count() > 0)
                    {
                        // Add the friend if they own the game
                        friendsWGame.Add(friend);
                    }
                }
                return friendsWGame;
            }
            throw  new SecurityAccessDeniedException();
        }

        public List<Status> GetStatusWithGame(Guid accessToken, int gameId)
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                // Get my friends with this game
                var friends = this.GetFriendsWithGame(accessToken, gameId);
                // Get their status about this game
                List<Status> status = new List<Status>();
                foreach (User friend in friends)
                {
                    var result = entities.Status.Where(r => ((r.UserID == friend.ID) && (r.GameNick.GameID == gameId)));
                    if (result.Count() > 0)
                    {
                        foreach (Status s in result)
                        {
                            s.UserReference.Load();
                            s.User.FacebookAccessToken = "";
                            s.User.GameNickAccessToken = null;
                        }
                        status.AddRange(result.ToList());
                    }
                }
                return status;
            }
            throw new SecurityAccessDeniedException();     
        }

        public List<Game> GetGames(Guid accessToken, int platformId)
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                var result = entities.Games.Where(game => game.PlatformID == platformId);
                return result.ToList();
            }
            throw new SecurityAccessDeniedException();
        }

        public List<Platform> GetPlatforms(Guid accessToken)
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                return entities.Platforms.ToList();
            }
            throw new SecurityAccessDeniedException();
        }

        public List<Status> GetStatus(Guid accessToken, int userId)
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                var result = entities.Status.Where(status => status.UserID == userId);
                foreach (Status status in result)
                {
                    status.GameNickReference.Load();
                    status.GameNick.GameReference.Load();
                }
                return result.ToList();
            }
            throw new SecurityAccessDeniedException();
        }

        public User RemoveGameNick(User accessToken, GameNick gameNick)
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                entities.Attach(gameNick);
                entities.DeleteObject(gameNick);
                entities.SaveChanges();
                return GetMe(entities, accessToken.ID);
            }
            throw new SecurityAccessDeniedException();
        }

        public User UpdateStatus(User accessToken, int gameNickId, string status)
        {
            GameNickEntities entities = new GameNickEntities();
            if (IsAuthenticated(accessToken, entities))
            {
                try
                {
                    Status newStatus = Status.CreateStatus(0, gameNickId, status, accessToken.ID);
                    entities.AddToStatus(newStatus);
                    entities.SaveChanges();
                    return GetMe(entities, accessToken.ID);
                }
                catch (Exception exception)
                {
                    throw;
                }
            }
            throw new SecurityAccessDeniedException();
        }

        #endregion
        #region HelperFunctions

        private bool IsAuthenticated(Guid accessToken, GameNickEntities entities)
        {
            var result = from u in entities.Users
                         where u.GameNickAccessToken == accessToken
                         select u;
            return result.Count() > 0;
        }

       
        private bool IsAuthenticated(User accessToken, GameNickEntities entities)
        {
            if (accessToken == null || entities == null)
            {
                //throw new Exception();
                return false;
            }
            var exists = from users in entities.Users
                         where users.GameNickAccessToken == accessToken.GameNickAccessToken
                         select users;
            return exists.Count() > 0;
        }
        private User GetMe(GameNickEntities entities, int me)
        {
            return entities.Users.Where(user => user.ID == me).First();
        }
        private User GetMe(GameNickEntities entities, Guid accessToken)
        {
            return entities.Users.Where(user => user.GameNickAccessToken == accessToken).First();
        }
        #endregion
    }
}
