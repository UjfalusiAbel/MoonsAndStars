using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonsAndStars.Assets.Code.Scripts.UI.Main
{
    public class MainMenu : MonoBehaviour
    {

        public void Play()
        {
            SceneManager.LoadScene("Space");
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}