namespace GameLogic
{
    public static partial class ActorEventHelper
    {
        public static void Send(EntityLogic actor, int eventId)
        {
            actor.Event.SendEvent(eventId);
        }

        public static void Send<TArg1>(EntityLogic actor, int eventId, TArg1 info)
        {
            actor.Event.SendEvent<TArg1>(eventId, info);
        }

        public static void Send<TArg1, TU>(EntityLogic actor, int eventId, TArg1 info1, TU info2)
        {
            actor.Event.SendEvent<TArg1, TU>(eventId, info1, info2);
        }

        public static void Send<TArg1, TU, TV>(EntityLogic actor, int eventId, TArg1 info1, TU info2, TV info3)
        {
            actor.Event.SendEvent<TArg1, TU, TV>(eventId, info1, info2, info3);
        }

        public static void Send<TArg1, TU, TV, TW>(EntityLogic actor, int eventId, TArg1 info1, TU info2, TV info3, TW info4)
        {
            actor.Event.SendEvent<TArg1, TU, TV, TW>(eventId, info1, info2, info3, info4);
        }
    }
}