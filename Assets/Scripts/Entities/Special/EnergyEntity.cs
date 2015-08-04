using UnityEngine;
using System.Collections;

public class EnergyEntity : Entity {

    //ENERGY PROPERTIES
    public float energyMax;
    public float energyCost;

    public Dummy energyDummy;

    //FUNCTIONS
    float energy;

    void Start()
    {
        RefreshDummy();
    }

    public bool HasEnoughEnergy()
    {
        return energy >= energyCost;
    }
    public void UseEnergy()
    {
        energy -= energyCost;
        RefreshDummy();
    }
    public float AddEnergy(float qty)
    {
        if (qty + energy > energyMax)
            qty = energyMax - energy;
        energy += qty;
        RefreshDummy();
        return qty;
    }
    public bool HasMaxEnergy()
    {
        return energy >= energyMax;
    }
    public float GetEnergyPercent()
    {
        return energy / energyMax;
    }

    public void RefreshDummy()
    {
        if (energyDummy != null)
            energyDummy.ScaleY(GetEnergyPercent());
    }
}
