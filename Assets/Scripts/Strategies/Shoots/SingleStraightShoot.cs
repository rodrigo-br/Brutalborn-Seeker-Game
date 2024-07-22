using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleStraightShoot : IShootStrategy
{
    public Gun Gun { get; }

    public SingleStraightShoot(Gun gun)
    {
        Gun = gun;
    }

    public void Use()
    {
        Gun.Shoot();
    }
}

public interface IShootStrategy : IStrategy
{
    Gun Gun { get; }
}
