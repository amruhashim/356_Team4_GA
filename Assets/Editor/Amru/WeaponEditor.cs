using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Weapon))]
public class WeaponEditor : Editor
{
    SerializedProperty playerCamera;
    SerializedProperty bulletPrefab;
    SerializedProperty bulletSpawn;
    SerializedProperty muzzleEffect;
    SerializedProperty GunType;
    SerializedProperty reloadTime;
    SerializedProperty magazineSize;
    SerializedProperty bulletsLeft;
    SerializedProperty isReloading;
    SerializedProperty bulletVelocity;
    SerializedProperty bulletLifeTime;
    SerializedProperty isShooting;
    SerializedProperty readyToShoot;
    SerializedProperty shootingDelay;
    SerializedProperty initialDelay;
    SerializedProperty bulletsPerShot;
    SerializedProperty spreadIntensity;
    SerializedProperty accumulatedBullets;
    SerializedProperty shootingSound;
    SerializedProperty reloadSound;
    SerializedProperty emptySound;

    private void OnEnable()
    {
        playerCamera = serializedObject.FindProperty("playerCamera");
        bulletPrefab = serializedObject.FindProperty("bulletPrefab");
        bulletSpawn = serializedObject.FindProperty("bulletSpawn");
        muzzleEffect = serializedObject.FindProperty("muzzleEffect");
        GunType = serializedObject.FindProperty("GunType");
        reloadTime = serializedObject.FindProperty("reloadTime");
        magazineSize = serializedObject.FindProperty("magazineSize");
        bulletsLeft = serializedObject.FindProperty("bulletsLeft");
        isReloading = serializedObject.FindProperty("isReloading");
        bulletVelocity = serializedObject.FindProperty("bulletVelocity");
        bulletLifeTime = serializedObject.FindProperty("bulletLifeTime");
        isShooting = serializedObject.FindProperty("isShooting");
        readyToShoot = serializedObject.FindProperty("readyToShoot");
        shootingDelay = serializedObject.FindProperty("shootingDelay");
        initialDelay = serializedObject.FindProperty("initialDelay");
        bulletsPerShot = serializedObject.FindProperty("bulletsPerShot");
        spreadIntensity = serializedObject.FindProperty("spreadIntensity");
        accumulatedBullets = serializedObject.FindProperty("accumulatedBullets");
        shootingSound = serializedObject.FindProperty("shootingSound");
        reloadSound = serializedObject.FindProperty("reloadSound");
        emptySound = serializedObject.FindProperty("emptySound");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(playerCamera);
        EditorGUILayout.PropertyField(bulletPrefab);
        EditorGUILayout.PropertyField(bulletSpawn);
        EditorGUILayout.PropertyField(muzzleEffect);

        EditorGUILayout.PropertyField(GunType);

        EditorGUILayout.PropertyField(reloadTime);
        EditorGUILayout.PropertyField(magazineSize);
        EditorGUILayout.PropertyField(bulletsLeft);
        EditorGUILayout.PropertyField(isReloading);

        EditorGUILayout.PropertyField(bulletVelocity);
        EditorGUILayout.PropertyField(bulletLifeTime);

        EditorGUILayout.PropertyField(isShooting);
        EditorGUILayout.PropertyField(readyToShoot);
        EditorGUILayout.PropertyField(shootingDelay);

        if ((Weapon.gunType)GunType.enumValueIndex == Weapon.gunType.MachineGun)
        {
            EditorGUILayout.PropertyField(initialDelay);
        }

        if ((Weapon.gunType)GunType.enumValueIndex == Weapon.gunType.ShotGun)
        {
            EditorGUILayout.PropertyField(bulletsPerShot);
        }

        EditorGUILayout.PropertyField(spreadIntensity);
        EditorGUILayout.PropertyField(accumulatedBullets);

        EditorGUILayout.PropertyField(shootingSound);
        EditorGUILayout.PropertyField(reloadSound);
        EditorGUILayout.PropertyField(emptySound);

        serializedObject.ApplyModifiedProperties();
    }
}
