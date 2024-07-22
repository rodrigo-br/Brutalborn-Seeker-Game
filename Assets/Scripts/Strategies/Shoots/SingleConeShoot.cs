using UnityEngine;

public class SingleConeShoot : IShootStrategy
{
    public Gun Gun { get; }
    private readonly float CONE_ANGLE;
    private Transform _bulletSpawnPosition;
    private float _defaultShootCooldown;

    public SingleConeShoot(Gun gun, float coneAngle = 30f)
    {
        Gun = gun;
        CONE_ANGLE = coneAngle;
        _bulletSpawnPosition = Gun.BulletSpawnPosition;
        _defaultShootCooldown = Gun.ShootCooldown;
        Gun.SetShootCooldown(0);
    }

    public void Use()
    {
        Quaternion originalRotation = _bulletSpawnPosition.transform.rotation;
        Gun.SetShootCooldown(0);
        _bulletSpawnPosition.transform.eulerAngles = new Vector3(0, 0, originalRotation.eulerAngles.z + CONE_ANGLE);
        Gun.Shoot();

        _bulletSpawnPosition.transform.eulerAngles = new Vector3(0, 0, originalRotation.eulerAngles.z - CONE_ANGLE);
        Gun.Shoot();

        _bulletSpawnPosition.transform.rotation = originalRotation;
        Gun.Shoot();
        Gun.SetShootCooldown(_defaultShootCooldown);
    }
}
