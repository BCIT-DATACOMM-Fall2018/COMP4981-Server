using System;

namespace GameStateComponents {
    public abstract class Actor  {
        private int actorId;
        private double[] position;

        public Actor(int actorId) {
            this.actorId = actorId;
            position = new double[2] {0, 0};
        }

        public int getActorId() {
            return actorId;
        }

        public double[] getPosition() {
            return position;
        }

        public void setPosition(double x, double y) {
            position[0] = x;
            position[1] = y;
        }

        public void setPosition(double[] position) {
            this.position[0] = position[0];
            this.position[1] = position[1];
        }
    }
}