﻿using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;
using GameStateComponents;

namespace Server
{
    public class ServerStateMessageBridge : IStateMessageBridge
    {
        State state;
        GameState gamestate;
        ClientManager clientmanager;

        public ServerStateMessageBridge()
        {
            state = State.Instance;
            gamestate = state.getGameState();
            clientmanager = state.getClientManger();
        }

        public void UpdateActorPosition(int actorId, float x, float z)
        {
            double[] pos = new double[2];
            pos[0] = x;
            pos[1] = z;

            gamestate.updatePosition(actorId, pos);
        }

        public void UpdateActorHealth(int actorId, int newHealth)
        {
            //Console.WriteLine("Actor Id: " + actorId + ", Health: " + newHealth);
            gamestate.addPlayer();
            gamestate.updateHealth(0, newHealth);
        }


        public void UseTargetedAbility(int actorId, AbilityType abilityType, int targetID)
        {
            Console.WriteLine("NOT YET IMPLEMENTED");
        }

        public void UseAreaAbility(int actorId, AbilityType abilityId, float x, float z)
        {
            Console.WriteLine("NOT YET IMPLEMENTED");
        }

        public void ProcessCollision(AbilityType abilityId, int actorHitId, int actorCastId)
        {
            Console.WriteLine("NOT YET IMPLEMENTED");
            //const buffer size = 1024
            // this is called when there exist a collison
            // check if this obj exist in buffer
            // if yes, increment the validity
            // if not, add

            // TODO: somewhere: every frame tick, decrement TTL
            // if TTL is 0 before the signal is sent, discard
            //TOOD: if valid >= x then signal collison function
        }

        public void SpawnActor(ActorType actorType, int ActorId, float x, float z)
        {
            double[] pos = new double[2];
            pos[0] = x;
            pos[1] = z;
            if (actorType != ActorType.Player)
            {
                Console.WriteLine("NOT YET IMPLEMENTED");
                return;
            }

            //currently just updates the position of the actor in the gamestate.
            //Should then send a reliable message to all clients to tell them something spawned
            gamestate.updatePosition(ActorId, pos);
        }

        public void SetActorMovement(int actorId, float x, float z, float targetX, float targetZ)
        {
            //send to all clients 
            Console.WriteLine("NOT YET IMPLEMENTED");
        }
    }
}