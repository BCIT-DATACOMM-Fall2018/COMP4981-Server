using System;
using NetworkLibrary;
using System.Collections.Concurrent;
using NetworkLibrary.MessageElements;
using GameStateComponents;

namespace Server
{
    public class CollisionItem
    {
        private const int maxTimeToLive = 20;

		public AbilityType abilityId { get; }
        public int actorHitId { get; }
        public int actorCastId { get; }
        public int validity { get; set; }
        public int timeToLive { get; set; } 
		public int collisionId { get; }

        public bool isSignalSent;

		public CollisionItem(AbilityType abilityId, int actorHitId, int actorCastId, int collisionId)
        {
            this.abilityId = abilityId;
            this.actorHitId = actorHitId;
            this.actorCastId = actorCastId;
            validity = 0;
            timeToLive = maxTimeToLive;
            isSignalSent = false;
			this.collisionId = collisionId;
        }

		public override bool Equals (object obj)
    	{
    		if (obj == null)
    			return false;
    		if (ReferenceEquals (this, obj))
    			return true;
    		if (obj.GetType () != typeof(CollisionItem))
    			return false;
    		CollisionItem other = (CollisionItem)obj;
			Console.WriteLine ("{0} = {1}", other.collisionId, collisionId );

			return abilityId == other.abilityId && actorHitId == other.actorHitId && actorCastId == other.actorCastId && collisionId == other.collisionId;
    	}
    	

    	public override int GetHashCode ()
    	{
    		unchecked {
    			return abilityId.GetHashCode () ^ actorHitId.GetHashCode () ^ actorCastId.GetHashCode ();
    		}
    	}
    	
    }


    public class CollisionBuffer
    {
        private const int MaxBufferSize = 1024;
		private const int maxValidity = 1;
	
		private readonly object padlock = new object();
		private CollisionItem[] buffer;
		private GameState gameState;

        private int currentBufferSize;
        private int tailPtr;
        private int headPtr;

		public CollisionBuffer(GameState gameState) {
			this.gameState = gameState;
			buffer = new CollisionItem[MaxBufferSize];
		}

        public void Insert(CollisionItem toAdd)
        {	
			lock (padlock) {
				if (!IsFull())
				{
					CollisionItem cur = ContainsBufferItem(toAdd);
					if (ContainsBufferItem(toAdd) == null)
					{
						Console.WriteLine ("Adding collision item to the queue");
						buffer[headPtr++] = toAdd;
						currentBufferSize++;
						cur = toAdd;
						Console.WriteLine ("Added collision head {0} , tail {1}", headPtr, tailPtr);
					}
					else
					{
						Console.WriteLine ("Incrementing validity of existing collision item");
					}
					if (++cur.validity == maxValidity || !cur.isSignalSent) {
						SignalCollision(cur);
					}
				}
				else
				{
					throw new OutOfMemoryException ("Not enough space in circular buffer to insert a new element.");
				}

				headPtr = headPtr % buffer.Length;
			}
        }

        private CollisionItem ContainsBufferItem(CollisionItem toAdd)
        {
			for (int i = tailPtr; i % MaxBufferSize != headPtr % MaxBufferSize; i++)
            {
                if (!buffer[i].Equals(toAdd)) continue;
                return buffer[i];
            }

            return null;
        }

        //TODO: called by frame tick
        public void DecrementTTL()
        {
			for (int i = tailPtr; i % MaxBufferSize != headPtr % MaxBufferSize; i++)
			{
				if (--buffer [i].timeToLive <= 0) {
					Console.WriteLine ("Discarding old collision item");
					Remove ();
				}
			}
		}

        private void SignalCollision(CollisionItem toSignal)
        {
			Console.WriteLine ("Signal collision {0}, {1}, {2}", toSignal.abilityId, toSignal.actorHitId, toSignal.actorCastId);
			toSignal.isSignalSent = true;
			gameState.TriggerAbilityEffects (toSignal.abilityId, toSignal.actorHitId, toSignal.actorCastId);
        }

        public CollisionItem Remove()
        {
			lock (padlock) {
				var itemToRemove = default(CollisionItem);
				if (!IsEmpty())
				{
					itemToRemove = buffer[tailPtr++];
					tailPtr = tailPtr % MaxBufferSize;
					currentBufferSize--;
				}
				else
				{
					throw new InvalidOperationException ("Attempted to remove an element from an empty circular buffer.");
				}

				return itemToRemove;
			}
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