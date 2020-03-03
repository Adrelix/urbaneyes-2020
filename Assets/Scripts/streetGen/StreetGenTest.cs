public class StreetGenTest {
    public Coordinate[] coords;
    public StreetGenTest(Coordinate[] coords){
        this.coords = coords;
    }

    public makeRoad(Coordinate coords) {
        // do road stuff
        foreach (Coordinate coord in coords) {
            
        }
    }
}

public class Coordinate {
    public float x, y, z;

    public Coordinate(float x,float y,float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}