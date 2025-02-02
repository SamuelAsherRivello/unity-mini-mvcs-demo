using RMC.Mini.Controller;
using RMC.BlockWorld.Mini.Model;
using RMC.BlockWorld.Mini.Model.Data;
using RMC.BlockWorld.Mini.Service;
using RMC.BlockWorld.Mini.Service.Storage;
using RMC.BlockWorld.Mini.View;
using RMC.BlockWorld.Standard;
using RMC.Mini;

namespace RMC.BlockWorld.Mini.Controller
{
    /// <summary>
    /// The Controller coordinates everything between
    /// the <see cref="IConcern"/>s and contains the core app logic 
    /// </summary>
    public class DeveloperConsoleController: BaseController // Extending 'base' is optional
        <BlockWorldModel, DeveloperConsoleView, LocalDiskStorageService> 
    {
        public DeveloperConsoleController(
            BlockWorldModel model, DeveloperConsoleView view, LocalDiskStorageService service) 
            : base(model, view, service)
        {
        }

        
        //  Initialization  -------------------------------
        public override void Initialize(IContext context)
        {
            if (!IsInitialized)
            {
                base.Initialize(context);

                //
                _view.OnReset.AddListener(View_OnReset);
                _view.OnRandomizeLanguage.AddListener(View_OnRandomizeLanguage);
                

                // Load the data as needed
                _service.OnLoadCompleted.AddListener(Service_OnLoadCompleted);
                if (!_model.HasLoadedService.Value)
                {
                    _service.Load();
                }
                else
                {
                    Service_OnLoadCompleted(null);
                }
            }
        }


        //  Methods ---------------------------------------

        
        //  Event Handlers --------------------------------
        
        private async void View_OnRandomizeLanguage()
        {
            RequireIsInitialized();
            await CustomLocalizationUtility.SetSelectedLocaleToNextAsync();
        }
            
        private void View_OnReset()
        {
            RequireIsInitialized();

            // Set from Random. Then save here.
            _model.CharacterData.Value = CharacterData.FromDefaultValues();
            _service.SaveCharacterData(_model.CharacterData.Value);
            
            _model.EnvironmentData.Value = EnvironmentData.FromDefaultValues();
            _service.SaveEnvironmentData(_model.EnvironmentData.Value);
        }
        
        private void Service_OnLoadCompleted(LocalDiskStorageServiceDto localDiskStorageServiceDto)
        {
            RequireIsInitialized();
            _model.HasLoadedService.Value = true;
            
            if (localDiskStorageServiceDto != null)
            {
                // Set FROM the saved data. Don't save again here.
                _model.CharacterData.Value = localDiskStorageServiceDto.CharacterData;
                _model.EnvironmentData.Value = localDiskStorageServiceDto.EnvironmentData;
            }
            else
            {
                _model.CharacterData.OnValueChangedRefresh();
                _model.EnvironmentData.OnValueChangedRefresh();
            }
        }
    }
}