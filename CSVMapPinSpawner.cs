// Inicializa variáveis e componentes relacionados à cena Unity e aos objetos
[SerializeField] private TooltipManager tooltipManager;
public GameObject textLabel;
public MapRenderer mapRenderer;
public MapPinLayer mapPinLayer;
public GameObject pinPrefab;
public Button switchCSVButton;
public Dropdown yearDropdown;
public Text buttonText;
public string pinVisualsName = "PinVisuals";
private float minValor = float.MaxValue;
private float maxValor = float.MinValue;

// Inicializa strings com os dados CSV de exemplo
private string csvData1 = @"...";
private string csvData2 = @"...";

// Inicializa variáveis e listas utilizadas ao longo do script
private List<MapPinData> mapPinDataList;
private List<GameObject> mapPins;
private bool usingFirstCSV = true;

// Função chamada no início da execução do script
private void Start()
{
        // Carrega os dados CSV, adiciona listeners aos elementos da UI e atualiza os pinos no mapa
        mapPinDataList = ParseCSV(csvData1);
        mapPins = new List<GameObject>();
        yearDropdown.onValueChanged.AddListener(delegate { UpdateMapPins(); });
        switchCSVButton.onClick.AddListener(SwitchCSV);
        UpdateMapPins();
        Debug.Log("CSV Loaded");
}

// Função para converter os dados CSV em uma lista de objetos MapPinData
private List<MapPinData> ParseCSV(string csv)
{
        List<MapPinData> dataList = new List<MapPinData>();
        string[] lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        // Processa cada linha do CSV, converte os valores e cria objetos MapPinData
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(';');
            if (values.Length >= 5)
            {
                double.TryParse(values[0], out double latitude);
                double.TryParse(values[1], out double longitude);
                int.TryParse(values[3], out int valor);
                int.TryParse(values[4], out int ano);

                MapPinData data = new MapPinData(latitude, longitude, values[2], valor, ano);
                dataList.Add(data);

                minValor = Mathf.Min(minValor, data.Valor);
                maxValor = Mathf.Max(maxValor, data.Valor);
            }
        }
        
        return dataList;
}

// Função para atualizar os pinos no mapa com base no ano selecionado
private void UpdateMapPins()
{
        int selectedYear = int.Parse(yearDropdown.options[yearDropdown.value].text);

        // Remove os pinos existentes e cria novos pinos com base nos dados filtrados pelo ano selecionado
        foreach (GameObject pin in mapPins)
        {
            Destroy(pin);
        }

        mapPins.Clear();

        mapPinLayer.MapPins.Clear(); // Clear the MapPins in the MapPinLayer

        float tamanhoMinimo = 0.2f;
        float tamanhoMaximo = 10.5f;
        
        foreach (MapPinData data in mapPinDataList)
        {
            if (data.Ano == selectedYear)
            {
                // Cria um novo objeto pino com base no prefab
                GameObject pin = Instantiate(pinPrefab);
                GameObject pinVisuals = pin.transform.Find(pinVisualsName).gameObject;
                GameObject pinVisualsPivot = pinVisuals.transform.Find("PinVisualsPivot").gameObject;
                
                // Calcula o tamanho do pino com base no valor normalizado e limita esse tamanho entre tamanhoMinimo e tamanhoMaximo
                float tamanho = Mathf.Clamp(normaliza(data.Valor, minValor, maxValor) * tamanhoMaximo, tamanhoMinimo, tamanhoMaximo);
                pinVisualsPivot.transform.localScale = new Vector3(1, tamanho, 1);
                
                // Obtém o componente MapPin do objeto "pin" e define sua localização com base nos dados de latitude e longitude
                MapPin mapPin = pin.GetComponent<MapPin>();
                mapPin.Location = new LatLon(data.Latitude, data.Longitude);

                // Adiciona o componente PinInteractionHandler ao objeto "pin" e define suas propriedades
                PinInteractionHandler pinInteractionHandler = pin.AddComponent<PinInteractionHandler>();
                pinInteractionHandler.textLabelPrefab = textLabel;
                pinInteractionHandler.Nome = data.Nome;
                pinInteractionHandler.Valor = data.Valor;
                

                // Adjust the position of the pin based on its scale
                //pinVisuals.transform.localPosition = new Vector3(0, tamanho / 2, 0);                

                // Set the parent of the pin to the mapPinLayer's transform.
                pin.transform.SetParent(mapPinLayer.transform, true);

               

                mapPins.Add(pin);
                mapPinLayer.MapPins.Add(mapPin); // Add the MapPin to the MapPinLayer.MapPins

                // Debug message showing the information for each pin being generated
                Debug.Log($"Pin generated for {data.Nome} ({data.Latitude}, {data.Longitude}) in {data.Ano} with value {data.Valor}");
            }
        }
}

// Função para normalizar o valor entre o mínimo e o máximo
private float normaliza(float valor, float min, float max)
{
        // Normaliza o valor utilizando a fórmula de normalização
        return (valor - min) / (max - min);

}

// Função para alternar entre os conjuntos de dados CSV
private void SwitchCSV()
{
        // Alterna entre os dois conjuntos de dados CSV e atualiza os pinos no mapa
        usingFirstCSV = !usingFirstCSV;
        buttonText.text = usingFirstCSV ? "Switch to Sales" : "Switch to Chargers";
        mapPinDataList = ParseCSV(usingFirstCSV ? csvData1 : csvData2);
        UpdateMapPins();
}

// Classe para armazenar informações sobre cada pino do mapa
public class MapPinData
{
// Define as variáveis para armazenar informações de cada pino
public double Latitude;
public double Longitude;
public string Nome;
public int Valor;
public int Ano;

// Construtor para inicializar um objeto MapPinData
public MapPinData(double latitude, double longitude, string nome, int valor, int ano)
{
    Latitude = latitude;
    Longitude = longitude;
    Nome = nome;
    Valor = valor;
    Ano = ano;
}
}
