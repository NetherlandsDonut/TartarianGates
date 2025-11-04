using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using static Core;
using System.Diagnostics;

public class Entity
{
    //Default constructor
    public Entity()
    {
        if (race != "") raceObj = Race.races.Find(x => x.name == race);
        if (background != "") backgroundObj = Background.backgrounds.Find(x => x.name == background);
    }

    //General entity constructor, when race is set to random it uses only the starting pool for players
    public Entity(string name = "", string gender = "", string race = "", string background = "")
    {
        if (race == "") race = Race.startingRaces[random.Next(Race.startingRaces.Count)].name;
        this.race = race;
        if (race != "") raceObj = Race.races.Find(x => x.name == race);
        stats = raceObj == null ? new() : raceObj.stats.ToDictionary(x => x.Key, x => x.Value);
        naturalResistances = raceObj.naturalResistances.ToDictionary(x => x.Key, x => x.Value);
        if (background == "")
        {
            var backgrounds = Background.backgrounds.FindAll(x => x.race == race);
            if (backgrounds.Count > 0) background = backgrounds[random.Next(backgrounds.Count)].name;
        }
        this.background = background;
        if (background != "") backgroundObj = Background.backgrounds.Find(x => x.name == background);
        bodySkills = backgroundObj == null ? new() : backgroundObj.bodySkills.ToDictionary(x => x.Key, x => x.Value);
        mindSkills = backgroundObj == null ? new() : backgroundObj.mindSkills.ToDictionary(x => x.Key, x => x.Value);
        soulSkills = backgroundObj == null ? new() : backgroundObj.soulSkills.ToDictionary(x => x.Key, x => x.Value);
        if (gender == "") gender = Roll(2) ? "Trans woman" : Roll(2) ? "Trans man" : Roll(2) ? "Non binary" : Roll(50) ? "Man" : "Woman";
        this.gender = gender;
        if (name == "") name = GenerateName();
        this.name = name;
    }

    //Prepares the entity to be used in the world
    public void Prepare()
    {
        if (id == 0) id = ++Save.save.map.newEntityID;
        skills ??= new();
    }

    //Generates a name based on currently set race and gender
    public string GenerateName()
    {
        var names = new List<Name>();
        if (gender == "Man" || gender == "Trans man") names = Name.names.Where(x => x.male).ToList();
        else if (gender == "Woman" || gender == "Trans woman") names = Name.names.Where(x => x.female).ToList();
        else if (gender == "Non binary") names = Name.names.Where(x => x.male && x.female).ToList();
        return names.Count == 0 ? null : names[random.Next(names.Count)].name;
    }

    //Provides the color with which the entity should be shown
    public string Color()
    {
        return "White";
    }

    //id of this entity
    public int id;

    //Coordinates of this entity
    public int x, y;

    //Name of this entity
    public string name;

    //Gender of this entity
    public string gender;

    //Race of this entity
    public string race;
    public Race raceObj;

    //Background of this entity
    public string background;
    public Background backgroundObj;

    //Team on which this entity is on
    public string team;

    //Stats of the entity
    public Dictionary<string, int> stats;

    //Natural resistances of the entity
    public Dictionary<string, int> naturalResistances;

    //Skills of the entity
    public Dictionary<string, int> bodySkills, mindSkills, soulSkills;

    #region Visual Representation

    //Get print
    public PreparedPrint GetPrint()
    {
        var temp = new PreparedPrint();
        temp = new("☻");
        return temp;
    }

    #endregion

    #region Skills

    //List of skills this entity has
    public Dictionary<string, int> skills;

    #endregion

    #region Calculations

    //How many units of time to wait for recovery for this entity
    //One unit of time is 1/10 of a second
    public int delay;

    //Cell this entity is on
    [NonSerialized] public Cell cell;

    //Cell this entity can see where it is
    [NonSerialized] public List<Cell> cellsSeen;

    //Calculates all the cells that this entity can see
    public void CalculateLOS()
    {
        if (cellsSeen != null)
            foreach (var cell in cellsSeen)
                cell.seenBy.Remove(this);
        cellsSeen = new() { cell };
        var sightRange = 10;
        for (int i = -sightRange; i <= sightRange; i++)
            for (int j = -sightRange; j <= sightRange; j++)
                for (int k = 0; k < 2; k++)
                    if (Math.Sqrt(i * i + j * j) <= sightRange)
                    {
                        var aimCell = cell.NeighboringCell(i, j);
                        if (aimCell == null) continue;
                        if (cellsSeen.Contains(aimCell)) continue;
                        var biggest = MathF.Abs(Math.Abs(i) > Math.Abs(j) ? i : j);
                        float curX = 0f, curY = 0f, curXR = 0, curYR = 0;
                        if (k == 0) while (true)
                        {
                            curXR = curX > 0 ? MathF.Floor(curX) : MathF.Ceiling(curX);
                            var checkedCell = cell.NeighboringCell((int)curXR, (int)curYR);
                            if (checkedCell == aimCell) { cellsSeen.Add(aimCell); break; }
                            else if (checkedCell != null && checkedCell.CanSeeThrough())
                            {
                                curX += i / biggest;
                                curYR = curY > 0 ? MathF.Floor(curY) : MathF.Ceiling(curY);
                                checkedCell = cell.NeighboringCell((int)curXR, (int)curYR);
                                if (checkedCell == aimCell) { cellsSeen.Add(aimCell); break; }
                                else if (checkedCell != null && checkedCell.CanSeeThrough()) curY += j / biggest;
                                else break;
                            }
                            else break;
                        }
                        else while (true)
                        {
                            curYR = curY > 0 ? MathF.Floor(curY) : MathF.Ceiling(curY);
                            var checkedCell = cell.NeighboringCell((int)curXR, (int)curYR);
                            if (checkedCell == aimCell) { cellsSeen.Add(aimCell); break; }
                            else if (checkedCell != null && checkedCell.CanSeeThrough())
                            {
                                curY += j / biggest;
                                curXR = curX > 0 ? MathF.Floor(curX) : MathF.Ceiling(curX);
                                checkedCell = cell.NeighboringCell((int)curXR, (int)curYR);
                                if (checkedCell == aimCell) { cellsSeen.Add(aimCell); break; }
                                else if (checkedCell != null && checkedCell.CanSeeThrough()) curX += i / biggest;
                                else break;
                            }
                            else break;
                        }
                    }
        foreach (var cell in cellsSeen)
            cell.seenBy.Add(this);
    }

    //Make a step through one unit of time with AI on this entity
    public void AIStep()
    {
        if (delay > 0) delay--;
        else
        {
            //Friends are all entities which are of the same team
            var friendsVisible = cellsSeen.FindAll(x => x.entities.Any(y => y.team == team));

            //Enemies are all entities which are not locals when this is a local one and the other way around
            var enemiesVisible = cellsSeen.FindAll(x => x.entities.Any(y => y.team.Contains("Locals") != team.Contains("Locals")));

            ////If any of the visible cells contains an enemy..
            //if (cellsSeen.)
            //{
            //    //If there is no task asigned to this entity
            //    //then look for a new one to be asigned
            //    if (asignedTask == null && taskDelay == 0)
            //    {
            //        //All tasks available for this entity
            //        var eligibleTasks = Save.save.world.tasks.Where(q => q.asignedEntity == 0 && !q.inactive && Save.save.world.regions[(q.posX / regionSize, q.posY / regionSize)].cells[q.posX % regionSize, q.posY % regionSize].visible/* && q.IsEntityValid(this)*/);

            //        //Is there a eligible task for this entity to do?
            //        if (eligibleTasks.Count() > 0)
            //        {
            //            //Pick the task that is aproximately closest
            //            Task closestTask = eligibleTasks.Aggregate((min, current) => Math.Abs(x - current.posX) + Math.Abs(y - current.posY) < Math.Abs(x - min.posX) + Math.Abs(y - min.posY) ? current : min);

            //            //If there is a task available for this entity
            //            //Pick up on it and set it as it's asigned task
            //            if (closestTask != null)
            //            {
            //                asignedTask = closestTask;
            //                closestTask.asignedEntity = id;
            //                pathSet = Path.ShortestPath(x, y, asignedTask.posX, asignedTask.posY, !closestTask.doneInCenter);

            //                //If the destination is unreachable cancel the asignment
            //                if (pathSet.movementMap == null)
            //                {
            //                    CancelAsignment(true);
            //                    taskDelay = 20;
            //                }

            //                //Otherwise proceed with it
            //                else plan = "Perform the asigned task";
            //            }
            //        }

            //        //If there wasn't such a task, apply a delay for looking again
            //        else taskDelay = 20;
            //    }
            //}

            ////If there is a plan..
            //if (plan != null)
            //{
            //    //If the plan is to perform a task but the task have turned inactive or removed from task list..
            //    if (plan == "Perform the asigned task" && (asignedTask.inactive || !Save.save.world.tasks.Contains(asignedTask)))
            //    {
            //        //Reset the plan and remove the assigned task
            //        CancelAsignment(false);
            //    }

            //    //If there still is a plan..
            //    if (plan != null)
            //    {
            //        //If haven't moved yet and there is a path to be walked
            //        if (pathSet != null && pathSet.movementMap.Count > 0)
            //        {
            //            var success = false;

            //            //Move entity towards the objective
            //            if (pathSet.movementMap[0].y > y && !success) success = Move(0, 1);
            //            else if (pathSet.movementMap[0].y < y && !success) success = Move(0, -1);
            //            if (pathSet.movementMap[0].x > x && !success) success = Move(1, 0);
            //            else if (pathSet.movementMap[0].x < x && !success) success = Move(-1, 0);

            //            //If entity is where the movement map is telling it to be
            //            if (pathSet.movementMap[0].x == x && pathSet.movementMap[0].y == y)
            //            {
            //                success = true;
            //                pathSet.movementMap.RemoveAt(0);
            //            }

            //            //If movement towards the objective failed then cancel the asignment
            //            if (!success) CancelAsignment(false);
            //        }

            //        //If entity arrived at the destination..
            //        if (pathSet != null && pathSet.movementMap.Count == 0)
            //        {
            //            if (plan == "Perform the asigned task")
            //            {
            //                var cell = Save.save.world.regions[(asignedTask.posX / regionSize, asignedTask.posY / regionSize)].cells[asignedTask.posX % regionSize, asignedTask.posY % regionSize];
            //                if (asignedTask.taskType == "Mine wall")
            //                {
            //                    cell.SetWall(0);
            //                }
            //                else if (asignedTask.taskType == "Dig hole")
            //                {
            //                    cell.SetGroundMined();
            //                }
            //                else if (asignedTask.taskType == "Harvest lumber")
            //                {
            //                    cell.SetWall(0);
            //                }
            //                else if (asignedTask.taskType == "Build wall")
            //                {
            //                    cell.SetWall(cell.ground.id);
            //                }
            //                else if (asignedTask.taskType == "Fill hole")
            //                {
            //                    cell.FillTheGround(cell.ground.id);
            //                }
            //                Save.save.world.tasks.Remove(asignedTask);
            //                asignedTask = null;
            //            }
            //            plan = null;
            //            pathSet = null;
            //        }
            //    }
            //}
        }
    }

    //Asigns this entity to a cell it's on
    public void AsignCell(Map map)
    {
        var cell = map.cells[x, y];
        if (!cell.entities.Contains(this))
            cell.entities.Add(this);
        this.cell = cell;
    }

    #endregion

    #region Movement

    //Moves the entity
    public bool Move(int X, int Y)
    {
        var newCell = cell.NeighboringCell(X, Y);
        if (newCell != null && newCell.IsWalkable())
        {
            cell.entities.Remove(this);
            newCell.entities.Add(this);
            cell = newCell;
            x += X;
            y += Y;
            CalculateLOS();
            return true;
        }
        return false;
    }

    //Opens the door in direction from the entity
    public bool OpenDoor(int X, int Y)
    {
        var newCell = cell.NeighboringCell(X, Y);
        if (newCell != null && newCell.wall != null && newCell.wall.isDoor && !newCell.wall.opened)
        {
            newCell.wall.opened = true;
            CalculateLOS();
            return true;
        }
        return false;
    }

    //Closes the door in direction from the entity
    public bool CloseDoor(int X, int Y)
    {
        var newCell = cell.NeighboringCell(X, Y);
        if (newCell != null && newCell.wall != null && newCell.wall.isDoor && newCell.wall.opened)
        {
            newCell.wall.opened = false;
            CalculateLOS();
            return true;
        }
        return false;
    }

    #endregion
}
