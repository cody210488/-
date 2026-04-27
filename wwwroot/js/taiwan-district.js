const taiwancities = [
    "台北市", "新北市", "桃園市", "台中市", "台南市", "高雄市",
    "基隆市", "新竹市", "嘉義市",
    "新竹縣", "苗栗縣", "彰化縣", "南投縣", "雲林縣", "嘉義縣",
    "屏東縣", "宜蘭縣", "花蓮縣", "台東縣", "澎湖縣", "金門縣", "連江縣"
];

const citySelect = document.getElementById("citySelect");
const districtSelect = document.getElementById("districtSelect");

//for (let i = 0; i < taiwancities.length;i++) {
//    const option = document.createElement("option");
//    option.value = taiwancities[i];
//    option.text = taiwancities[i];
//    citySelect.appendChild(option);
//}
taiwancities.forEach(city => {
    const option = document.createElement("option");
    option.value = city;
    option.text = city;
    citySelect.appendChild(option);
})
citySelect.addEventListener("change", function () {
    const selectedCity = this.value;

    districtSelect.innerHTML = '<option value="">請選擇地區</option>';
    fetch(`/Courts/GetDistricts?city=${encodeURIComponent(selectedCity)}`).then(response => response.json()).then(data => {

        data.forEach(district => {
            const option = document.createElement("option");
            option.value = district;
            option.text = district;

            districtSelect.appendChild(option);
        });
    });
    
});