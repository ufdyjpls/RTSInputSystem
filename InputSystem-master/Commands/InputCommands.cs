using RTS.Controlls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Visuals;
using UI.MainGame.Views.Info;

namespace RTS.Controlls
{
    public partial class InputCommandManager {

        //центровать камеру к начальной позиции
        [InputCommand(InputCommand.CameraCenter)]
        public void OnCameraCenter()
        {

        }

        //открыть общий чат
        [InputCommand(InputCommand.ChatOpenGeneral)]
        public void OnChatOpenGeneral()
        {
            if (ToogleInterface.Instance)
            {
                ToogleInterface.Instance.ToogleChat(0);
            }
        }

        //открыть командный чат
        [InputCommand(InputCommand.ChatOpenTeam)]
        public void OnChatOpenTeam()
        {
            if (ToogleInterface.Instance)
            {
                ToogleInterface.Instance.ToogleChat(1);
            }
        }

        //ответить на сообщение
        [InputCommand(InputCommand.ChatResponse)]
        public void OnChatResponse()
        {

        }

        //зыкрыть чат
        [InputCommand(InputCommand.ChatClose)]
        public void OnChatClose()
        {
            if (ToogleInterface.Instance)
            {
                ToogleInterface.Instance.CloseChat();
            }
        }

        //открыть статистику????
        [InputCommand(InputCommand.GeneralOpenGameInfo)]
        public void OnGeneralOpenGameInfo()
        {
            //TEMP THING!!!!
            if (ToogleInterface.Instance)
            {
                ToogleInterface.Instance.TooglePanel(ToogleInterface.Instance.menuHandlerPanel);
            }
        }

        //открыть меню
        [InputCommand(InputCommand.GeneralOpenGameMenu)]
        public void OnGeneralOpenGameMenu()
        {
            if (ToogleInterface.Instance)
            {
                ToogleInterface.Instance.TooglePanel(ToogleInterface.Instance.menuPanel);
            }
        }

        //открыть передачу ресурсов
        [InputCommand(InputCommand.GeneralOpenTeamTrade)]
        public void OnGeneralOpenTeamTrade()
        {
            if (ToogleInterface.Instance)
            {
                ToogleInterface.Instance.TooglePanel(ToogleInterface.Instance.allyPanel);
            }
        }

        //пауза игры (только одиночка)
        [InputCommand(InputCommand.GeneralPause)]
        public void OnGeneralPause()
        {

        }

        //открыть миникарту
        [InputCommand(InputCommand.GeneralToggleMinimap)]
        public void OnGeneralToggleMinimap()
        {
            if (ToogleInterface.Instance)
            {
                ToogleInterface.Instance.TooglePanel(ToogleInterface.Instance.minimapPanel);
            }
        }

        //вкл/выкл звуков
        [InputCommand(InputCommand.GeneralToggleSound)]
        public void OnGeneralToggleSound()
        {

        }

        //открыть/закрыть ВЕСЬ интерфейс
        [InputCommand(InputCommand.GeneralToggleInterface)]
        public void OnGeneralToggleInterface()
        {
            if(ToogleInterface.Instance){
                ToogleInterface.Instance.TooglePanel(ToogleInterface.Instance.mainPanel);
            }
        }

        [InputCommand(InputCommand.GeneralToggleMinimapTreeAndRock)]
        public void OnGeneralToggleMinimapTreeAndRock()
        {
            if (Minimap.Instance)
            {
                Minimap.Instance.ApplyRocksAndTrees = !Minimap.Instance.ApplyRocksAndTrees;
            }
        }

        [InputCommand(InputCommand.GeneralToggleMinimapMine)]
        public void OnGeneralToggleMinimapMine()
        {
            if (Minimap.Instance)
            {
                Minimap.Instance.ApplyMines = !Minimap.Instance.ApplyMines;
            }
        }

        [InputCommand(InputCommand.GeneralTogglePlayerInfo)]
        public void OnGeneralTogglePlayerInfo()
        {
            if(InfoPlayersView.Instance)
            {
                if (InfoPlayersView.Instance.PlayerPanel.activeSelf)
                {
                    InfoPlayersView.Instance.PlayerPanel.SetActive(false);
                }
                else
                {
                    InfoPlayersView.Instance.PlayerPanel.SetActive(true);
                }
            }
        }

        [InputCommand(InputCommand.GeneralToggleUnitColor)]
        public void OnGeneralToggleUnitColor()
        {
            if (Minimap.Instance)
            {
                Minimap.Instance.ApplySingleColorForAlliesAndEnemies = !Minimap.Instance.ApplySingleColorForAlliesAndEnemies;
            }
        }

        [InputCommand(InputCommand.SelectionAddCommand)]
        public void OnSelectionAddCommand()
        {

        }

        [InputCommand(InputCommand.SelectionKill)]
        public void OnSelectionKill()
        {

        }

        [InputCommand(InputCommand.SelectionSelectAll)]
        public void OnSelectionSelectAll()
        {

        }

        [InputCommand(InputCommand.SelectionSelectAllSoldier)]
        public void OnSelectionSelectAllSoldier()
        {

        }

        [InputCommand(InputCommand.SelectionSelectAllFleet)]
        public void OnSelectionSelectAllFleet()
        {

        }

        [InputCommand(InputCommand.SelectionSelectAllBuildings)]
        public void OnSelectionSelectAllBuildings()
        {

        }

        [InputCommand(InputCommand.SelectionSelectAllType)]
        public void OnSelectionSelectAllType()
        {

        }

        [InputCommand(InputCommand.SelectionSelectAllFreeWorker)]
        public void OnSelectionSelectAllFreeWorker()
        {

        }

        [InputCommand(InputCommand.SelectionSelectAllFreeMine)]
        public void OnSelectionSelectAllFreeMine()
        {

        }

        [InputCommand(InputCommand.TrainingProduce1)]
        public void OnTrainingProduce1()
        {

        }

        [InputCommand(InputCommand.TrainingProduce5)]
        public void OnTrainingProduce5()
        {

        }

        [InputCommand(InputCommand.TrainingProduce100)]
        public void OnTrainingProduce100()
        {

        }

        [InputCommand(InputCommand.TrainingRemove1)]
        public void OnTrainingRemove1()
        {

        }

        [InputCommand(InputCommand.TrainingRemove5)]
        public void OnTrainingRemove5()
        {

        }

        [InputCommand(InputCommand.TrainingRemove100)]
        public void OnTrainingRemove100()
        {

        }

        [InputCommand(InputCommand.TrainingRemoveaAll)]
        public void OnTrainingRemoveaAll()
        {

        }

        [InputCommand(InputCommand.WarDisableAttack)]
        public void OnWarDisableAttack()
        {

        }

        [InputCommand(InputCommand.WarEnableAttack)]
        public void OnWarEnableAttack()
        {

        }

        [InputCommand(InputCommand.WarHold)]
        public void OnWarHold()
        {

        }

        [InputCommand(InputCommand.WarDismiss)]
        public void OnWarDismiss()
        {

        }

        [InputCommand(InputCommand.WarGuard)]
        public void OnWarGuard()
        {

        }

        [InputCommand(InputCommand.WarPatrol)]
        public void OnWarPatrol()
        {

        }

        [InputCommand(InputCommand.WarSetGroup1)]
        public void OnWarSetGroup1()
        {
            SelectHandler.Instance().SquadInSelectionControll(1, true);
            BindPanelView.Instance.SetBindByIndex(1);
            Debug.Log("InputCommand.WarSetGroup1");
        }

        [InputCommand(InputCommand.WarSetGroup2)]
        public void OnWarSetGroup2()
        {
            SelectHandler.Instance().SquadInSelectionControll(2, true);
            BindPanelView.Instance.SetBindByIndex(2);
            Debug.Log("InputCommand.WarSetGroup2");
        }

        [InputCommand(InputCommand.WarSetGroup3)]
        public void OnWarSetGroup3()
        {
            SelectHandler.Instance().SquadInSelectionControll(3, true);
            BindPanelView.Instance.SetBindByIndex(3);
            Debug.Log("InputCommand.WarSetGroup3");
        }

        [InputCommand(InputCommand.WarSetGroup4)]
        public void OnWarSetGroup4()
        {
            SelectHandler.Instance().SquadInSelectionControll(4, true);
            BindPanelView.Instance.SetBindByIndex(4);
            Debug.Log("InputCommand.WarSetGroup4");
        }

        [InputCommand(InputCommand.WarSetGroup5)]
        public void OnWarSetGroup5()
        {
            SelectHandler.Instance().SquadInSelectionControll(5, true);
            BindPanelView.Instance.SetBindByIndex(5);
            Debug.Log("InputCommand.WarSetGroup5");
        }

        [InputCommand(InputCommand.WarSetGroup6)]
        public void OnWarSetGroup6()
        {
            SelectHandler.Instance().SquadInSelectionControll(6, true);
            BindPanelView.Instance.SetBindByIndex(6);
            Debug.Log("InputCommand.WarSetGroup6");
        }

        [InputCommand(InputCommand.WarSetGroup7)]
        public void OnWarSetGroup7()
        {
            SelectHandler.Instance().SquadInSelectionControll(7, true);
            BindPanelView.Instance.SetBindByIndex(7);
            Debug.Log("InputCommand.WarSetGroup7");
        }

        [InputCommand(InputCommand.WarSetGroup8)]
        public void OnWarSetGroup8()
        {
            SelectHandler.Instance().SquadInSelectionControll(8, true);
            BindPanelView.Instance.SetBindByIndex(8);
            Debug.Log("InputCommand.WarSetGroup8");
        }

        [InputCommand(InputCommand.WarSetGroup9)]
        public void OnWarSetGroup9()
        {
            SelectHandler.Instance().SquadInSelectionControll(9, true);
            BindPanelView.Instance.SetBindByIndex(9);
            Debug.Log("InputCommand.WarSetGroup9");
        }

        [InputCommand(InputCommand.WarSetGroup0)]
        public void OnWarSetGroup0()
        {
            SelectHandler.Instance().SquadInSelectionControll(0, true);
            BindPanelView.Instance.SetBindByIndex(0);
            Debug.Log("InputCommand.WarSetGroup0");
        }

        [InputCommand(InputCommand.WarGetGroup1)]
        public void OnWarGetGroup1()
        {
            SelectHandler.Instance().SquadInSelectionControll(1, false);
            Debug.Log("InputCommand.WarGetGroup1");
        }

        [InputCommand(InputCommand.WarGetGroup2)]
        public void OnWarGetGroup2()
        {
            SelectHandler.Instance().SquadInSelectionControll(2, false);
            Debug.Log("InputCommand.WarGetGroup2");
        }

        [InputCommand(InputCommand.WarGetGroup3)]
        public void OnWarGetGroup3()
        {
            SelectHandler.Instance().SquadInSelectionControll(3, false);
            Debug.Log("InputCommand.WarGetGroup3");
        }

        [InputCommand(InputCommand.WarGetGroup4)]
        public void OnWarGetGroup4()
        {
            SelectHandler.Instance().SquadInSelectionControll(4, false);
            Debug.Log("InputCommand.WarGetGroup4");
        }

        [InputCommand(InputCommand.WarGetGroup5)]
        public void OnWarGetGroup5()
        {
            SelectHandler.Instance().SquadInSelectionControll(5, false);
            Debug.Log("InputCommand.WarGetGroup5");
        }

        [InputCommand(InputCommand.WarGetGroup6)]
        public void OnWarGetGroup6()
        {
            SelectHandler.Instance().SquadInSelectionControll(6, false);
            Debug.Log("InputCommand.WarGetGroup6");
        }

        [InputCommand(InputCommand.WarGetGroup7)]
        public void OnWarGetGroup7()
        {
            SelectHandler.Instance().SquadInSelectionControll(7, false);
            Debug.Log("InputCommand.WarGetGroup7");
        }

        [InputCommand(InputCommand.WarGetGroup8)]
        public void OnWarGetGroup8()
        {
            SelectHandler.Instance().SquadInSelectionControll(8, false);
            Debug.Log("InputCommand.WarGetGroup8");
        }

        [InputCommand(InputCommand.WarGetGroup9)]
        public void OnWarGetGroup9()
        {
            SelectHandler.Instance().SquadInSelectionControll(9, false);
            Debug.Log("InputCommand.WarGetGroup9");
        }

        [InputCommand(InputCommand.WarGetGroup0)]
        public void OnWarGetGroup0()
        {
            SelectHandler.Instance().SquadInSelectionControll(0, false);
            Debug.Log("InputCommand.WarGetGroup0");
        }

        [InputCommand(InputCommand.BuildHome)]
        public void OnBuildHome()
        {
            Debug.Log("InputCommand.BuildHome");
        }

        [InputCommand(InputCommand.BuildMill)]
        public void OnBuildMill()
        {
            Debug.Log("InputCommand.BuildMill");
        }

        [InputCommand(InputCommand.BuildWarehouse)]
        public void OnBuildWarehouse()
        {
            Debug.Log("InputCommand.BuildWarehouse");
        }

        [InputCommand(InputCommand.BuildIronMine)]
        public void OnBuildMine()
        {
            Debug.Log("InputCommand.BuildIronMine");
        }

        [InputCommand(InputCommand.BuildTemple)]
        public void OnBuildTemple()
        {
            Debug.Log("InputCommand.BuildTemple");
        }

        [InputCommand(InputCommand.BuildPort)]
        public void OnBuildPort()
        {
            Debug.Log("InputCommand.BuildPort");
        }

        [InputCommand(InputCommand.BuildMarket)]
        public void OnBuildMarket()
        {
            Debug.Log("InputCommand.BuildMarket");
        }

        [InputCommand(InputCommand.BuildBarracks17)]
        public void OnBuildCasern17()
        {
            Debug.Log("InputCommand.BuildBarracks17");
        }

        [InputCommand(InputCommand.BuildBarracks18)]
        public void OnBuildCasern18()
        {
            Debug.Log("InputCommand.BuildBarracks18");
        }

        [InputCommand(InputCommand.BuildForge)]
        public void OnBuildForge()
        {
            Debug.Log("InputCommand.BuildForge");
        }

        [InputCommand(InputCommand.BuildStable)]
        public void OnBuildStable()
        {
            Debug.Log("InputCommand.BuildStable");
        }

        [InputCommand(InputCommand.BuildArtilleryDepot)]
        public void OnBuildArtilleryDepot()
        {
            Debug.Log("InputCommand.BuildArtilleryDepot");
        }

        [InputCommand(InputCommand.BuildAcademy)]
        public void OnBuildAcademy()
        {
            Debug.Log("InputCommand.BuildAcademy");
        }

        [InputCommand(InputCommand.BuildDiplomaticCenter)]
        public void OnBuildDiplomaticCenter()
        {
            Debug.Log("InputCommand.BuildDiplomaticCenter");
        }

        [InputCommand(InputCommand.BuildCityCenter)]
        public void OnBuildSityCenter()
        {
            Debug.Log("InputCommand.BuildCityCenter");
        }

        [InputCommand(InputCommand.BuildRockWall)]
        public void OnBuildRockWall()
        {
            Debug.Log("InputCommand.BuildRockWall");
        }
    }
}
