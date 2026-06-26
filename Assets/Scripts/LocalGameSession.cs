using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using static SkyMavisLogin;

public static class LocalGameSession
{
    public const string WalletAddress = "local-steam-player";
    public const string PlayerName = "Player";
    public const string RunId = "local-run";

    private const int MaxRounds = 13;

    public static bool IsActive { get; private set; }

    private static readonly StarterMonsterData[] StarterMonsters =
    {
        new StarterMonsterData("11817626", "aquatic", "0x200000000000010001c080c102100000000100002841020800010008284142080001000028008202000100083080820c000100083041850a0001000810408208"),
        new StarterMonsterData("11770530", "dawn", "0x880000000000010000815010830000000001001428a1450a000100042860410a00010000182100060001001430a18006000100083080c10a0001001010808106"),
        new StarterMonsterData("11714634", "bug", "0x80000000000010001400080830800000001000420014104000100041080400a000100041020830a000100042061430c000100043021840a000100041820c006"),
        new StarterMonsterData("11695797", "beast", "0x10001c080c1040800000001000808410208000100082841020a00010008182184080001000c2841820c000100082041020c0001000810408204"),
        new StarterMonsterData("11695796", "bird", "0x100000000000010002418070821000000001000820410304000100082860420a000100081840c206000100083061420c000100081841020c0001000810418204"),
        new StarterMonsterData("11685884", "plant", "0x180000000000010002412041030800000001000820410208000100082820420a000100081840c206000100083041850600010008304100040001000828408204"),
        new StarterMonsterData("11690909", "mech", "0x80000000000001000380c070c40400000001001428a1450a0001000010010002000100001860840c0001000c08a1400c000100101860c0040001001010808404"),
        new StarterMonsterData("11393577", "dusk", "0x900000000000010001c0a0c08208000000010000202081020001000c08208302000100102000c206000100043060800a0000030030a1800a0001000c0800450c"),
        new StarterMonsterData("11591641", "plant", "0x180000000000010001018050820800000001000420204102000100101001010200010004200101020001000430a1000c0001000c302141060001000008814304"),
        new StarterMonsterData("11410369", "mech", "0x800000000000010001c020c1021000000001000c084101040001000c08a08008000100042020c10600010004304041060001000c300080040001000c08410008")
    };

    private static readonly Vector2Int[] OpponentBoardPositions =
    {
        new Vector2Int(0, 3),
        new Vector2Int(1, 3),
        new Vector2Int(2, 3),
        new Vector2Int(0, 4),
        new Vector2Int(1, 4)
    };

    public static Root CreatePlayerRoot()
    {
        IsActive = true;
        PartFinder.LoadFromResources();

        return new Root
        {
            userInfo = new UserInfo
            {
                addr = WalletAddress,
                email = "",
                name = PlayerName,
                roninAddress = WalletAddress,
                user_id = WalletAddress
            },
            monsters = new NftsResponse
            {
                page = "1",
                page_size = StarterMonsters.Length.ToString(),
                cursor = "",
                result = StarterMonsters.Select(CreateMonsterItem).ToArray()
            },
            lands = new NftsResponse
            {
                page = "1",
                page_size = "0",
                cursor = "",
                result = Array.Empty<Item>()
            }
        };
    }

    public static string GetOpponentJson(int score)
    {
        IsActive = true;
        PartFinder.LoadFromResources();

        StarterMonsterData[] opponentMonsters = StarterMonsters
            .Skip(StarterMonsters.Length - 5)
            .Take(5)
            .ToArray();

        StarterMonsterData captain = opponentMonsters[score % opponentMonsters.Length];
        LandType landType = (LandType)(score % Enum.GetValues(typeof(LandType)).Length);

        Opponent opponent = new Opponent
        {
            user_wallet_address = "local-opponent",
            username = "Local Rival",
            monster_captain_id = captain.Id,
            monster_captain_genes = captain.Genes,
            land_type = (int)landType,
            monster_team = new MonsterTeamDatabase
            {
                monsters = opponentMonsters
                    .Select((monster, index) => CreateBackendMonster(monster, OpponentBoardPositions[index]))
                    .ToArray(),
                team_upgrades_values_per_round = CreateEmptyUpgradeRounds()
            }
        };

        return JsonConvert.SerializeObject(opponent);
    }

    public static string GetLeaderboardJson()
    {
        StarterMonsterData captain = GetCaptainData();
        var response = new LeaderboardResponseDTO
        {
            data = new List<LeaderboardDTO>
            {
                new LeaderboardDTO
                {
                    username = PlayerName,
                    avg_wins = 0,
                    elo = 1000,
                    monster_captain_id = captain.Id,
                    monster_captain_genes = captain.Genes,
                    user_wallet_address = WalletAddress
                },
                new LeaderboardDTO
                {
                    username = "Local Rival",
                    avg_wins = 6,
                    elo = 980,
                    monster_captain_id = StarterMonsters[5].Id,
                    monster_captain_genes = StarterMonsters[5].Genes,
                    user_wallet_address = "local-opponent"
                }
            }
        };

        return JsonConvert.SerializeObject(response);
    }

    private static Item CreateMonsterItem(StarterMonsterData monster)
    {
        string monsterClass = GetClass(monster);
        RawMetadata metadata = new RawMetadata
        {
            external_url = "",
            genes = monster.Genes,
            id = long.Parse(monster.Id),
            image = "",
            name = $"Monster #{monster.Id}",
            title = "",
            properties = new Properties
            {
                monster_id = long.Parse(monster.Id),
                birthdate = 0,
                bodyshape = "normal",
                breed_count = 0,
                @class = monsterClass,
                ears_id = "",
                eyes_id = "",
                horn_id = "",
                mouth_id = "",
                back_id = "",
                tail_id = "",
                primary_color = $"{monsterClass}-01",
                stage = 4
            }
        };

        return new Item
        {
            contractAddress = "local",
            f2p = false,
            createdAtBlock = 0,
            createdAtBlockTime = "",
            metadata = JsonConvert.SerializeObject(metadata),
            token_id = monster.Id,
            name = "Monster",
            tokenStandard = "LOCAL",
            symbol = "MONSTER",
            token_uri = "",
            updatedAtBlock = 0,
            updatedAtBlockTime = ""
        };
    }

    private static MonsterForBackend CreateBackendMonster(StarterMonsterData monster, Vector2Int boardPosition)
    {
        int[] comboIds = GetComboIds(monster);

        return new MonsterForBackend
        {
            monster_id = monster.Id,
            genes = monster.Genes,
            position_values_per_round = Enumerable.Range(0, MaxRounds)
                .Select(_ => new Position { row = boardPosition.x, col = boardPosition.y })
                .ToArray(),
            combos_values_per_round = Enumerable.Range(0, MaxRounds)
                .Select(_ => new Combos { combos_id = comboIds.ToArray() })
                .ToArray(),
            upgrades_values_per_round = Array.Empty<int>()
        };
    }

    private static UpgradeValuesPerRound[] CreateEmptyUpgradeRounds()
    {
        return Enumerable.Range(0, MaxRounds)
            .Select(_ => new UpgradeValuesPerRound { upgrades_ids = Array.Empty<UpgradeAugument>() })
            .ToArray();
    }

    private static int[] GetComboIds(StarterMonsterData monster)
    {
        try
        {
            List<GetMonstersExample.Part> parts = CreateParts(monster);
            int[] comboIds = parts
                .Where(part => part.BodyPart != BodyPart.Ears && part.BodyPart != BodyPart.Eyes)
                .Select(part => (int)part.SkillName)
                .Distinct()
                .Take(2)
                .ToArray();

            return comboIds.Length > 0 ? comboIds : new[] { (int)SkillName.Chubby };
        }
        catch
        {
            return new[] { (int)SkillName.Chubby };
        }
    }

    private static List<GetMonstersExample.Part> CreateParts(StarterMonsterData monster)
    {
        List<string> partClasses = MonsterGeneUtils.GetMonsterPartsClasses(monster.Genes);
        List<string> partAbilities = MonsterGeneUtils.ParsePartIdsFromHex(monster.Genes);

        if (partClasses == null || partAbilities == null || partClasses.Count < 6 || partAbilities.Count < 6)
            return new List<GetMonstersExample.Part>();

        return new List<GetMonstersExample.Part>
        {
            new GetMonstersExample.Part(partClasses[2], "", "horn", 0, false, partAbilities[2]),
            new GetMonstersExample.Part(partClasses[5], "", "tail", 0, false, partAbilities[5]),
            new GetMonstersExample.Part(partClasses[4], "", "back", 0, false, partAbilities[4]),
            new GetMonstersExample.Part(partClasses[3], "", "mouth", 0, false, partAbilities[3])
        };
    }

    private static StarterMonsterData GetCaptainData()
    {
        string captainId = PlayerPrefs.GetString("Captain" + WalletAddress);
        StarterMonsterData captain = StarterMonsters.FirstOrDefault(monster => monster.Id == captainId);
        return captain.Id != null ? captain : StarterMonsters[0];
    }

    private static string GetClass(StarterMonsterData monster)
    {
        try
        {
            return MonsterGeneUtils.GetMonsterClass(monster.Genes).ToString().ToLowerInvariant();
        }
        catch
        {
            return monster.ClassName;
        }
    }

    private readonly struct StarterMonsterData
    {
        public StarterMonsterData(string id, string className, string genes)
        {
            Id = id;
            ClassName = className;
            Genes = genes;
        }

        public string Id { get; }
        public string ClassName { get; }
        public string Genes { get; }
    }
}
