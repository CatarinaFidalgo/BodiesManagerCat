using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    private string ConfigFile;
    
    

    // Local
    public BodiesManager localBodiesManager;
    public UdpBodiesListener localUdpBodiesListener;
    public TrackerMesh localRavatarManager;
    public Transform localOrigin;


    public bool loadRavatar;


    public void Start()
    {
        ConfigFile = Application.dataPath + "/config.txt";


        // 1. Ler cenas do config (ex. portas udp...)
        // 2. iniciar componentes 


        int localTrackerBroadcastPort = int.Parse(ConfigProperties.load(ConfigFile, "tracker.broadcast.port"));
        int localTrackerListenPort = int.Parse(ConfigProperties.load(ConfigFile, "tracker.listen.port"));
        int localAvatarListenPort = int.Parse(ConfigProperties.load(ConfigFile, "client.avatar.listen.port"));

        localUdpBodiesListener.startListening(localTrackerBroadcastPort);

        if (loadRavatar)
        {
            localRavatarManager.Init(
                    localTrackerListenPort,
                    localAvatarListenPort,
                    localOrigin
                    );
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            localBodiesManager.calibrateHuman();
        }
    }
}
