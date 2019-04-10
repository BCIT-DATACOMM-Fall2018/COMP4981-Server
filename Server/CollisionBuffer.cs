using System;
using NetworkLibrary;
using System.Collections.Concurrent;
using NetworkLibrary.MessageElements;
using GameStateComponents;

namespace Server
{
	/// ----------------------------------------------
	/// Class: 		CollisionItem - A Class that models a collision element in the CollisionBuffer.
	/// 
	/// PROGRAM:	Server
	///
	/// 
	/// FUNCTIONS:	public CollisionItem (AbilityType abilityId, int actorHitId, int actorCastId, int collisionId)
	/// 			public override bool Equals (object obj)
	///				public override int GetHashCode ()
	/// 
	/// DATE: 		March 11, 2019
	///
	/// REVISIONS: 
	///
	/// DESIGNER: 	Daniel Shin
	///
	/// PROGRAMMER: Daniel Shin, Cameron Roberts
	///
	/// NOTES:			
	///				This class is responsible for holding the relavent data for a collision.
	/// ----------------------------------------------		
    public class CollisionItem
    {
        private const int maxTimeToLive = 20;

		public AbilityType abilityId { get; }
        public int actorHitId { get; }
        public int actorCastId { get; }
        public int validity { get; set; }
        public int timeToLive { get; set; }
		public int collisionId { get; }
        private static Logger Log;

        public bool isSignalSent;

		/// ----------------------------------------------
		/// FUNCTION:		CollisionItem
		/// 
		/// DATE:			March 11, 2019
		/// 
		/// REVISIONS:		(none)
		/// 
		/// DESIGNER:	 	Daniel Shin
		/// 
		/// PROGRAMMER:		Daniel Shin, Cameron Roberts
		/// 
		/// INTERFACE: 		public CollisionItem (AbilityType abilityId, int actorHitId, int actorCastId, int collisionId)
		/// 
		/// RETURNS: 		void
		/// 
		/// NOTES:		  	CollisionItem constructor.
		/// ----------------------------------------------
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

		/// ----------------------------------------------
		/// FUNCTION:		Equals
		/// 
		/// DATE:			March 11, 2019
		/// 
		/// REVISIONS:		(none)
		/// 
		/// DESIGNER:	 	Daniel Shin
		/// 
		/// PROGRAMMER:		Daniel Shin, Cameron Roberts
		/// 
		/// INTERFACE: 		public override bool Equals (object obj)
		/// 
		/// RETURNS: 		bool; returns true if two CollisionItem is the same, false otherwise.
		/// 
		/// NOTES:		  	A overridden Equals method for CollisionItem.
		/// ----------------------------------------------
		public override bool Equals (object obj)
    	{
    		if (obj == null)
    			return false;
    		if (ReferenceEquals (this, obj))
    			return true;
    		if (obj.GetType () != typeof(CollisionItem))
    			return false;
    		CollisionItem other = (CollisionItem)obj;
            Log.V(other.collisionId + " = " + collisionId );

			return abilityId == other.abilityId && actorHitId == other.actorHitId && actorCastId == other.actorCastId && collisionId == other.collisionId;
    	}

		/// ----------------------------------------------
		/// FUNCTION:		GetHashCode
		/// 
		/// DATE:			March 11, 2019
		/// 
		/// REVISIONS:		(none)
		/// 
		/// DESIGNER:	 	Daniel Shin
		/// 
		/// PROGRAMMER:		Daniel Shin, Cameron Roberts
		/// 
		/// INTERFACE: 		public override int GetHashCode ()
		/// 
		/// RETURNS: 		int; returns the computed hash code for the CollisionItem.
		/// 
		/// NOTES:		  	A overridden GetHashCode method for CollisionItem.
		///					Hashcode is computed XORing the hashcode of:
		///						abilityId - ability that was casted to cause the collision
		///						actorHitId - the actor that was hit
		///						actorCastId - the actor that casted the ability
		/// ----------------------------------------------
    	public override int GetHashCode ()
    	{
    		unchecked {
    			return abilityId.GetHashCode () ^ actorHitId.GetHashCode () ^ actorCastId.GetHashCode ();
    		}
    	}
    }


	/// ----------------------------------------------
	/// Class: 		CollisionBuffer - A Circular buffer to hold the CollisionItem to be processed.
	/// 
	/// PROGRAM:	Server
	/// 
	/// FUNCTIONS:	public CollisionBuffer (GameState gameState)
	/// 			public void Insert (CollisionItem toAdd)
	///				private CollisionItem ContainsBufferItem (CollisionItem toAdd)
	///				public void DecrementTTL ()
	///				private void SignalCollision(CollisionItem toSignal)
	///				public CollisionItem Remove ()
	///				private bool IsEmpty ()
	///				private bool IsFull ()
	/// 
	/// DATE: 		March 11, 2019
	///
	/// REVISIONS: 
	///
	/// DESIGNER: 	Daniel Shin, Cameron Roberts
	///
	/// PROGRAMMER: Daniel Shin, Cameron Roberts
	///
	/// NOTES:			
	///				This class is responsible for holding the CollisionItems in a circular buffer.
	///				The circular buffer can hold 1024 CollisionItems and is implemented using an 
	///				array with two pointers: head and tail to keep track of the first and last CollisionItems.
	/// ----------------------------------------------
    public class CollisionBuffer
    {
        private const int MaxBufferSize = 1024;
		public int requiredValidity = 1;

		private readonly object padlock = new object();
		private CollisionItem[] buffer;
		private GameState gameState;
        private static Logger Log;

        private int currentBufferSize;
        private int tailPtr;
        private int headPtr;

		/// ----------------------------------------------
		/// FUNCTION:		CollisionBuffer
		/// 
		/// DATE:			March 11, 2019
		/// 
		/// REVISIONS:		(none)
		/// 
		/// DESIGNER:	 	Daniel Shin
		/// 
		/// PROGRAMMER:		Daniel Shin, Cameron Roberts
		/// 
		/// INTERFACE: 		public CollisionBuffer (GameState gameState)
		/// 
		/// RETURNS: 		void
		/// 
		/// NOTES:		  	CollisionBuffer constructor.
		/// ----------------------------------------------
		public CollisionBuffer(GameState gameState) {
			this.gameState = gameState;
			buffer = new CollisionItem[MaxBufferSize];
		}

		/// ----------------------------------------------
		/// FUNCTION:		Insert
		/// 
		/// DATE:			March 11, 2019
		/// 
		/// REVISIONS:		(none)
		/// 
		/// DESIGNER:	 	Daniel Shin, Cameron Roberts
		/// 
		/// PROGRAMMER:		Daniel Shin, Cameron Roberts
		/// 
		/// INTERFACE: 		public void Insert (CollisionItem toAdd)
		/// 
		/// RETURNS: 		void
		/// 
		/// NOTES:		  	The function inserts a CollisionItem to the circular buffer if the 
		///					item does not already exist in the buffer and the buffer is not full.
		/// ----------------------------------------------
        public void Insert(CollisionItem toAdd)
        {
			lock (padlock) {
				if (!IsFull())
				{
					CollisionItem cur = ContainsBufferItem(toAdd);
					if (ContainsBufferItem(toAdd) == null)
					{
						Log.V("Adding collision item to the queue");
						buffer[headPtr++] = toAdd;
						currentBufferSize++;
						cur = toAdd;
						Log.V("Added collision head " + headPtr + ", tail " + tailPtr);
					}
					else
					{
                        Log.V("Incrementing validity of existing collision item. Validity " + cur.validity);
					}
					if (++cur.validity == requiredValidity && !cur.isSignalSent) {
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

		/// ----------------------------------------------
		/// FUNCTION:		ContainsBufferItem
		/// 
		/// DATE:			March 11, 2019
		/// 
		/// REVISIONS:		(none)
		/// 
		/// DESIGNER:	 	Daniel Shin
		/// 
		/// PROGRAMMER:		Daniel Shin, Cameron Roberts
		/// 
		/// INTERFACE: 		private CollisionItem ContainsBufferItem (CollisionItem toAdd) 
		/// 
		/// RETURNS: 		CollisionItem; returns the CollisionItem in the buffer we are looking for,
		///					null if it does not exist.
		/// 
		/// NOTES:		  	The function searches for a CollisionItem in the buffer and returns it if it exists
		///					and returns null if it does not.
		/// ----------------------------------------------
        private CollisionItem ContainsBufferItem(CollisionItem toAdd)
        {
			for (int i = tailPtr; i % MaxBufferSize != headPtr % MaxBufferSize; i++)
            {
                if (!buffer[i].Equals(toAdd)) continue;
                return buffer[i];
            }

            return null;
        }

        /// ----------------------------------------------
		/// FUNCTION:		DecrementTTL
		/// 
		/// DATE:			March 11, 2019
		/// 
		/// REVISIONS:		(none)
		/// 
		/// DESIGNER:	 	Daniel Shin
		/// 
		/// PROGRAMMER:		Daniel Shin, Cameron Roberts
		/// 
		/// INTERFACE: 		public void DecrementTTL ()
		/// 
		/// RETURNS: 		void
		/// 
		/// NOTES:		  	The function decreaments the time to live of all the
		///					CollisionItem every frame tick, and discards them if the TTL
		///					is 0.
		/// ----------------------------------------------
        public void DecrementTTL()
        {
			for (int i = tailPtr; i % MaxBufferSize != headPtr % MaxBufferSize; i++)
			{
				if (--buffer [i].timeToLive <= 0) {
					Log.V("Discarding old collision item");
					Remove ();
				}
			}
		}

        private void SignalCollision(CollisionItem toSignal)
        {
			Log.V("Signal collision " + toSignal.abilityId + ", " + toSignal.actorHitId + ", " + toSignal.actorCastId);
			toSignal.isSignalSent = true;
			gameState.TriggerAbility (toSignal.abilityId, toSignal.actorHitId, toSignal.actorCastId);
        }

		/// ----------------------------------------------
		/// FUNCTION:		Remove
		/// 
		/// DATE:			March 11, 2019
		/// 
		/// REVISIONS:		(none)
		/// 
		/// DESIGNER:	 	Daniel Shin
		/// 
		/// PROGRAMMER:		Daniel Shin, Cameron Roberts
		/// 
		/// INTERFACE: 		public CollisionItem Remove () 
		/// 
		/// RETURNS: 		CollisionItem; returns the oldest CollisionItem just removed from the buffer.
		/// 
		/// NOTES:		  	The function removes the oldest CollisionItem just removed from the buffer,
		///					if the buffer is not already empty.
		/// ----------------------------------------------
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

		/// ----------------------------------------------
		/// FUNCTION:		IsEmpty
		/// 
		/// DATE:			March 11, 2019
		/// 
		/// REVISIONS:		(none)
		/// 
		/// DESIGNER:	 	Daniel Shin
		/// 
		/// PROGRAMMER:		Daniel Shin, Cameron Roberts
		/// 
		/// INTERFACE: 		private bool IsEmpty () 
		/// 
		/// RETURNS: 		bool; returns true if the buffer has a size of 0, false otherwise.
		/// 
		/// NOTES:		  	The function simply checks if the buffer is empty.
		/// ----------------------------------------------
        private bool IsEmpty()
        {
            return currentBufferSize == 0;
        }

		/// ----------------------------------------------
		/// FUNCTION:		IsFull
		/// 
		/// DATE:			March 11, 2019
		/// 
		/// REVISIONS:		(none)
		/// 
		/// DESIGNER:	 	Daniel Shin
		/// 
		/// PROGRAMMER:		Daniel Shin, Cameron Roberts
		/// 
		/// INTERFACE: 		private bool IsFull () 
		/// 
		/// RETURNS: 		bool; returns true if the buffer is full, false otherwise.
		/// 
		/// NOTES:		  	The function simply checks if the buffer is full.
		/// ----------------------------------------------
        private bool IsFull()
        {
            return currentBufferSize == MaxBufferSize;
        }
    }
}
