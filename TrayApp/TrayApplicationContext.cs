using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using CleanDrop.Core.Models;          
using CleanDrop.Core.Services;       
using CleanDrop.Core.Repositories;    

namespace TrayApp
{

    /// Contexto de la aplicación que gestiona el icono en la bandeja del sistema

    public class TrayApplicationContext : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _contextMenu;
        private MultiWatcherService _watcherService;
        private AppConfig _config;
        private int _archivosOrganizados = 0;

        public TrayApplicationContext()
        {
            // Cargar configuración
            _config = ConfigurationManager.CargarConfig();

            // Inicializar servicio de monitoreo
            InicializarServicio();

            // Crear icono en la bandeja
            CrearTrayIcon();

            // Si está configurado para iniciar automáticamente, iniciar monitoreo
            if (_config.IniciarMonitoreoAutomatico)
            {
                IniciarMonitoreo();
            }
        }


        /// Inicializa el servicio de monitoreo de archivos

        private void InicializarServicio()
        {
            try
            {
                var ruleRepo = new RuleRepository();
                var reglas = ruleRepo.ObtenerReglas();
                var ruleEngine = new RuleEngine(reglas);

                _watcherService = new MultiWatcherService(ruleEngine);

                // Suscribirse a eventos
                _watcherService.ArchivoOrganizado += OnArchivoOrganizado;
                _watcherService.ErrorOcurrido += OnErrorOcurrido;
            }
            catch (Exception ex)
            {
                MostrarError($"Error al inicializar el servicio:\n{ex.Message}");
            }
        }

        /// Crea el icono en la bandeja del sistema

        private void CrearTrayIcon()
        {
            // Crear menú contextual
            _contextMenu = new ContextMenuStrip();
            ActualizarMenu();

            // Crear icono
            _trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Application,
                ContextMenuStrip = _contextMenu,
                Visible = true,
                Text = "CleanDrop - Detenido"
            };

            // Evento de doble clic: abrir consola
            _trayIcon.DoubleClick += (sender, e) => AbrirConsola();
        }

        /// Actualiza el menú contextual según el estado actual

        private void ActualizarMenu()
        {
            _contextMenu.Items.Clear();

            // Encabezado
            var headerItem = new ToolStripLabel("🗂️ CleanDrop")
            {
                Font = new Font(_contextMenu.Font, FontStyle.Bold)
            };
            _contextMenu.Items.Add(headerItem);
            _contextMenu.Items.Add(new ToolStripSeparator());

            // Estado actual
            bool estaActivo = _watcherService?.EstaActivo ?? false;

            if (estaActivo)
            {
                var pausarItem = new ToolStripMenuItem("⏸ Pausar monitoreo", null, OnPausarMonitoreo);
                _contextMenu.Items.Add(pausarItem);
            }
            else
            {
                var iniciarItem = new ToolStripMenuItem("▶ Iniciar monitoreo", null, OnIniciarMonitoreo);
                _contextMenu.Items.Add(iniciarItem);
            }

            _contextMenu.Items.Add(new ToolStripSeparator());

            // Estadísticas
            var carpetasActivas = _watcherService?.CarpetasActivas ?? 0;
            var estadoItem = new ToolStripMenuItem("📊 Estado") { Enabled = false };
            var carpetasInfo = new ToolStripMenuItem($"   📂 {carpetasActivas} carpeta(s) monitoreada(s)") { Enabled = false };
            var archivosInfo = new ToolStripMenuItem($"   ✓ {_archivosOrganizados} archivo(s) organizados") { Enabled = false };

            _contextMenu.Items.Add(estadoItem);
            _contextMenu.Items.Add(carpetasInfo);
            _contextMenu.Items.Add(archivosInfo);
            _contextMenu.Items.Add(new ToolStripSeparator());

            // Opciones
            _contextMenu.Items.Add(new ToolStripMenuItem("🖥️ Abrir panel de control", null, OnAbrirConsola));
            _contextMenu.Items.Add(new ToolStripMenuItem("📋 Ver reglas", null, OnVerReglas));
            _contextMenu.Items.Add(new ToolStripMenuItem("📁 Abrir carpeta de configuración", null, OnAbrirCarpetaConfig));
            _contextMenu.Items.Add(new ToolStripSeparator());

            // Opciones de inicio
            var inicioWindowsItem = new ToolStripMenuItem("🔄 Iniciar con Windows")
            {
                Checked = _config.IniciarConWindows,
                CheckOnClick = true
            };
            inicioWindowsItem.Click += OnToggleInicioWindows;

            _contextMenu.Items.Add(inicioWindowsItem);
            _contextMenu.Items.Add(new ToolStripSeparator());

            // Salir
            _contextMenu.Items.Add(new ToolStripMenuItem("❌ Salir", null, OnSalir));
        }

        #region Eventos del Servicio

        private void OnArchivoOrganizado(object? sender, ArchivoOrganizadoEventArgs e)
        {
            _archivosOrganizados++;
            bool estaActivo = _watcherService?.EstaActivo ?? false;
            _trayIcon.Text = $"CleanDrop - {(estaActivo ? "Activo" : "Detenido")}\n{_archivosOrganizados} archivos organizados";
            ActualizarMenu();
        }

        private void OnErrorOcurrido(object? sender, ErrorEventArgs e)
        {
            // Modo silencioso por defecto
        }

        #endregion

        #region Manejadores de Menú

        private void OnIniciarMonitoreo(object? sender, EventArgs e) => IniciarMonitoreo();
        private void OnPausarMonitoreo(object? sender, EventArgs e) => PausarMonitoreo();
        private void OnAbrirConsola(object? sender, EventArgs e) => AbrirConsola();

        private void OnVerReglas(object? sender, EventArgs e)
        {
            try
            {
                var ruleRepo = new RuleRepository();
                var reglas = ruleRepo.ObtenerReglas();

                string mensaje = "═══ REGLAS CONFIGURADAS ═══\n\n";
                foreach (var regla in reglas)
                {
                    mensaje += $"📁 {regla.Nombre}\n";
                    mensaje += $"   Extensiones: {string.Join(", ", regla.Extensiones)}\n";
                    mensaje += $"   Destino: {regla.CarpetaDestino}\n\n";
                }

                MessageBox.Show(mensaje, "Reglas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar reglas:\n{ex.Message}");
            }
        }

        private void OnAbrirCarpetaConfig(object? sender, EventArgs e)
        {
            ConfigurationManager.AbrirCarpetaConfig();
        }

        private void OnToggleInicioWindows(object? sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null) return;

            _config.IniciarConWindows = item.Checked;
            ConfigurationManager.GuardarConfig(_config);

            if (item.Checked)
            {
                ConfigurarInicioConWindows(true);
            }
            else
            {
                ConfigurarInicioConWindows(false);
            }
        }

        private void OnSalir(object? sender, EventArgs e)
        {
            var resultado = MessageBox.Show(
                "¿Estás seguro de que quieres salir?\n\nEl monitoreo de archivos se detendrá.",
                "Salir",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (resultado == DialogResult.Yes)
            {
                PausarMonitoreo();
                _trayIcon.Visible = false;
                Application.Exit();
            }
        }

        #endregion

        #region Métodos Auxiliares

        private void IniciarMonitoreo()
        {
            try
            {
                _watcherService.Iniciar(_config.CarpetaMonitoreada);
                _trayIcon.Text = "CleanDrop - Activo";
                ActualizarMenu();
            }
            catch (Exception ex)
            {
                MostrarError($"Error al iniciar monitoreo:\n{ex.Message}");
            }
        }

        private void PausarMonitoreo()
        {
            try
            {
                _watcherService?.Detener();
                _trayIcon.Text = "CleanDrop - Detenido";
                ActualizarMenu();
            }
            catch (Exception ex)
            {
                MostrarError($"Error al pausar monitoreo:\n{ex.Message}");
            }
        }

        private void AbrirConsola()
        {
            try
            {
                string consoleExeName = "CleanDrop.Console.exe";
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, consoleExeName);

                if (File.Exists(exePath))
                {
                    Process.Start(exePath);
                    return;
                }

                var archivosDisponibles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe");

                MessageBox.Show(
                    $"No se encontró: {consoleExeName}\n\n" +
                    $"Ubicación: {AppDomain.CurrentDomain.BaseDirectory}\n\n" +
                    "Archivos .exe disponibles:\n" +
                    string.Join("\n", archivosDisponibles.Select(f => Path.GetFileName(f))),
                    "Panel de control no encontrado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                MostrarError($"Error al abrir el panel de control:\n{ex.Message}");
            }
        }

        private void ConfigurarInicioConWindows(bool activar)
        {
            try
            {
                string appName = "CleanDrop";
                string exePath = Application.ExecutablePath;

                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        if (activar)
                        {
                            key.SetValue(appName, $"\"{exePath}\"");
                        }
                        else
                        {
                            key.DeleteValue(appName, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error al configurar inicio con Windows:\n{ex.Message}");
            }
        }

        private void MostrarError(string mensaje)
        {
            MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _watcherService?.Detener();
                _watcherService?.Dispose();
                _trayIcon?.Dispose();
                _contextMenu?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}