using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;

public class FakeMonstersManager : MonoBehaviour
{
    public List<FakeMonsterController> instantiatedMonsters = new List<FakeMonsterController>();
    public MonsterSpawner monsterSpawner;
    public FakeMapManager mapManager;
    public FakeLandManager fakeLandManager;
    static public FakeMonstersManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    IEnumerator Start()
    {
        mapManager.GenerateFakeMap();
        InitializeMonsters();
        while (AccountManager.userLands == null)
        {
            yield return null;
        }

        fakeLandManager.ChooseFakeLand(AccountManager.userLands.results.FirstOrDefault(X => X.LandTypeEnum == RunManagerSingleton.instance.landType).token_id);
    }

    private void InitializeMonsters()
    {
        int index = 0;
        foreach (var fakeMonsterController in instantiatedMonsters)
        {
            Vector2Int? gridLocation = null;
            FakeOverlayTile startingTile = fakeMonsterController.standingOnTile;

            while (startingTile == null || startingTile.occupied)
            {
                if (gridLocation == null)
                {
                    gridLocation = new Vector2Int(0, index);
                }

                startingTile = mapManager.map[gridLocation.Value];
                if (startingTile.occupied)
                {
                    gridLocation = null;
                    index++;
                }
            }

            PositionCharacterOnTile(fakeMonsterController, startingTile);
        }
    }

    private void PositionCharacterOnTile(FakeMonsterController character, FakeOverlayTile tile)
    {
        character.transform.position = tile.transform.position;
        tile.occupied = true;
        tile.currentOccupier = character;
        character.standingOnTile = tile;
    }

    public void PositionCharacterOnTile(string monsterId, Position position)
    {
        FakeOverlayTile tile = mapManager.map[new Vector2Int(position.row, position.col)];
        FakeMonsterController character = instantiatedMonsters.FirstOrDefault(x => x.monster != null && x.monster.id == monsterId);
        if (character == null)
            return;
        character.transform.position = tile.transform.position;
        tile.occupied = true;
        tile.currentOccupier = character;
        character.standingOnTile = tile;
    }

    public void ClearAllMonsters()
    {
        int index = 0;
        foreach (var fakeMonsterController in instantiatedMonsters)
        {
            if (fakeMonsterController.monster == null)
            {
                continue;
            }

            Vector2Int? gridLocation = null;
            gridLocation = new Vector2Int(0, index);
            fakeMonsterController.standingOnTile = mapManager.map[gridLocation.Value];
            Position position = new Position();
            position = new Position() { row = 0, col = gridLocation.Value.y };
            PositionCharacterOnTile(fakeMonsterController.monster.id, position);
            fakeMonsterController.SetVisible(false);
            fakeMonsterController.monster = null;
            index++;
        }
    }

    public void RemoveMonster(string MonsterId)
    {
        var fakeMonsterController = instantiatedMonsters.Single(x => x.monster != null && x.monster.id == MonsterId);
        fakeMonsterController.SetVisible(false);
        fakeMonsterController.monster = null;
    }

    public MonsterVisualDescriptor GetMonsterArt(GetMonstersExample.Monster monster)
    {
        return monsterSpawner.ProcessMixer(monster);
    }

    public MonsterVisualDescriptor ChooseMonster(GetMonstersExample.Monster monster)
    {
        var monsterControllerAlreadySelected =
            instantiatedMonsters.FirstOrDefault(x => x.monster != null && x.monster.id == monster.id);
        if (monsterControllerAlreadySelected != null)
        {
            RemoveMonster(monsterControllerAlreadySelected.monster.id);
            return null;
        }

        if (instantiatedMonsters.Any(x => !x.visible))
        {
            MonsterVisualDescriptor descriptor = monsterSpawner.ProcessMixer(monster);
            var fakeMonsterController =
                instantiatedMonsters.FirstOrDefault(x => !x.visible);

            fakeMonsterController.monster = monster;
            fakeMonsterController.SetDescriptor(descriptor);
            fakeMonsterController.SetVisible(true);
            return descriptor;
        }
        else
        {
            Debug.Log("Full Monsters");
            return null;
        }
    }
}
