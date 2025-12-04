using UnityEngine;

namespace Quizly
{
    public class GameLoader : MonoBehaviour
    {
        [SerializeField] private GameSettings gameSetting;

        private void Start()
        {
            DBManager.LoadDatabase(gameSetting.databaseName);
        }
    }
}
