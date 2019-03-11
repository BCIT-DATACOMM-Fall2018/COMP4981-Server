using System;

namespace GameStateComponents {
    public abstract class Actor {
        private int actorId;
        private float[] position;
        private float[] targetPosition;

        public Actor(int actorId) {
            this.actorId = actorId;
            position = new float[2] {0, 0};
        }

        public int getActorId() {
            return actorId;
        }

        public float[] getPosition() {
            return position;
        }

        public void setPosition(float x, float y) {
            position[0] = x;
            position[1] = y;
        }

        public float[] getTargetPosition()
        {
            return targetPosition;
        }

        public void setTargetPosition(float x, float y)
        {
            targetPosition[0] = x;
            targetPosition[1] = y;
        }

        public void setPosition(float[] position) {
            this.position[0] = position[0];
            this.position[1] = position[1];
        }
    }
}