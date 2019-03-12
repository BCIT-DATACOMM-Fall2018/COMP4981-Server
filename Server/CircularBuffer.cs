using System;
using NetworkLibrary;

namespace Server
{
    public class BufferItem
    {
        private const int maxTimeToLive = 10;

        AbilityType abilityId;
        public int actorHitId { get; }
        public int actorCastId { get; }
        public int validity { get; set; }
        public int timeToLive { get; set; } //TODO: decrement every frame tick

        public bool isSignalSent; //TODO:

        public BufferItem(AbilityType abilityId, int actorHitId, int actorCastId)
        {
            this.abilityId = abilityId;
            this.actorHitId = actorHitId;
            this.actorCastId = actorCastId;
            validity = 0;
            timeToLive = maxTimeToLive;
            isSignalSent = false;
        }
    }

    //TODO: if TTL is 0 before the signal is sent, discard

    public class CircularBuffer
    {
        private static CircularBuffer instance;

        private static BufferItem[] buffer;

        private const int MaxBufferSize = 1024;

        private int currentBufferSize;

        private int tailPtr;

        private int headPtr;

        private const int maxValidity = 5;

        public static CircularBuffer Instance
        {
            get
            {
                buffer = new BufferItem[MaxBufferSize];
                return instance ?? (instance = new CircularBuffer());
            }
        }

        public void Insert(BufferItem toAdd)
        {
            if (!IsFull())
            {
                BufferItem cur = ContainsBufferItem(toAdd);
                if (ContainsBufferItem(toAdd) == null)
                {
                    buffer[headPtr++] = toAdd;
                    currentBufferSize++;
                }
                else
                {
                    if (++cur.validity == maxValidity)
                        SignalCollision(cur);
                }
            }
            else
            {
                Console.Write("Buffer Over Flow");
            }

            headPtr = headPtr % buffer.Length;
        }

        private BufferItem ContainsBufferItem(BufferItem toAdd)
        {
            for (int i = 0; i < currentBufferSize; i++)
            {
                if (!buffer[i].Equals(toAdd)) continue;
                return buffer[i];
            }

            return null;
        }

        //TODO: called by frame tick
        public void DecrementTTL(BufferItem toDecrement)
        {
            ContainsBufferItem(toDecrement).timeToLive--;
        }

        private void SignalCollision(BufferItem toAdd)
        {
            //TODO: notify collision 
        }

        public BufferItem Remove()
        {
            var itemToRemove = default(BufferItem);
            if (!IsEmpty())
            {
                itemToRemove = buffer[tailPtr++];
                tailPtr = tailPtr % MaxBufferSize;
                currentBufferSize--;
            }
            else
            {
                Console.Write("Buffer Under Flow");
            }

            return itemToRemove;
        }

        private bool IsEmpty()
        {
            return currentBufferSize == 0;
        }

        private bool IsFull()
        {
            return currentBufferSize == MaxBufferSize;
        }
    }
}