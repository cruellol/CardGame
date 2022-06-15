namespace Cards
{
	public enum CardUnitType : byte
	{
		None = 0,
		Murloc = 1,
		Beast = 2,
		Elemental = 3,
		Mech = 4
	}

	public enum SideType : byte
	{
		Common = 0,
		Mage = 1,
		Warrior = 2
	}

	public enum CardStateType: byte
    {
		InDeck,
		InHand,
		OnTable,
		Descard
    }

	public enum CardFeature : byte
    {
		OtherMurlocks_A1,
		Damage_1,
		Taunt,
		Charge,
		Restrore_2,
		Summon_Murcloc,
		Summon_Boar,
		Draw_1,
		Other_A1,
		Friendly_A1H1,
		Summon_Dagonling,
		RestoreAll_2,
		GainForOtherDriendly_A1H1,
		GainAfterDamage_A3,
		HeroDamage_3,
		Damage_2,
		Other_A1H1
    }
}
