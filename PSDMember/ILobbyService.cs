using PSD.Base.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace PSDMember
{
    [ServiceKnownType("GetKnownTypes", typeof(Helper))]
    public interface ILobbyService
    {
        [OperationContract(IsInitiating = true)]
        LoginStatus Login(string userName, string hashPass, int version, out Account account,
            out string reconnectionString, out LoginToken loginToken);
        [OperationContract(IsInitiating = false)]
        void Logout();

        [OperationContract]
        IEnumerable<Room> ListRooms();
        [OperationContract]
        IEnumerable<Room> ListFreeRooms();

        [OperationContract]
        Room CreateRoom(Diva settings, string password = null);

        [OperationContract]
        RoomOperationResult EnterRoom(int roomId, bool spectate, string password, out Room room);

        [OperationContract]
        RoomOperationResult ExitRoom();

        [OperationContract]
        RoomOperationResult Wind(int newSeat);

        [OperationContract]
        RoomOperationResult Ready();

        [OperationContract]
        RoomOperationResult Kick(int seat);

        [OperationContract]
        RoomOperationResult OpenSeat(int seat);

        [OperationContract]
        RoomOperationResult CloseSeat(int seat);

        [OperationContract]
        RoomOperationResult Chat(string message);

        [OperationContract]
        RoomOperationResult Spectate(int room);

        [OperationContract]
        LoginStatus CreateAccount(string userName, string password);
        // TODO: should we support create account here (in lobby?)

        [OperationContract]
        void SubmitBugReport(System.IO.Stream s);
    }

    public interface IGameClient
    {
        [OperationContract(IsOneWay = true)]
        void NotifyRoomUpdate(int id, Room room);

        [OperationContract(IsOneWay = true)]
        void NotifyKicked();

        [OperationContract(IsOneWay = true)]
        void NotifyGameStart(string connectionString, LoginToken token);

        [OperationContract(IsOneWay = true)]
        void NotifyChat(Account account, string message);

        [OperationContract]
        bool Ping();
    }

    internal static class Helper
    {
        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
        {
            return new Type[] { typeof(Room), typeof(Seat), typeof(Account) }.ToList();
        }
    }
}
