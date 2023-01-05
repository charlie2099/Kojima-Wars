public enum EBaseState
{
    IDLE, // Base is owned and not contested
    CONTESTED, // Two teams are inside the zone
    UNCONTESTED, // Only one team is inside the zone but the zone does not belong to them yet
    //COOLDOWN // Base not owned but capture progress is regressing as no players are inside it
}