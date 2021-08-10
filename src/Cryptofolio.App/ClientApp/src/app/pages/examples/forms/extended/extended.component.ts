import { Component, OnInit } from "@angular/core";
import noUiSlider from "nouislider";

@Component({
  selector: "app-extended",
  templateUrl: "extended.component.html"
})
export class ExtendedComponent implements OnInit {
  mytime: Date = new Date();
  tags = ["Amsterdam", "Washington", "Sydney", "Beijing"];
  date: Date = new Date();
  switch = true;
  switch1 = true;
  switch2 = true;
  iconOff: string = '<i class="tim-icons icon-simple-remove"></i>';
  iconOn: string = '<i class="tim-icons icon-check-2"></i>';

  isDropup = true;
  constructor() {}

  ngOnInit() {
    var slider = document.getElementById("sliderRegular");

    noUiSlider.create(slider, {
      start: 40,
      connect: false,
      range: {
        min: 0,
        max: 100
      }
    });

    var slider2 = document.getElementById("sliderDouble");

    noUiSlider.create(slider2, {
      start: [20, 60],
      connect: true,
      range: {
        min: 0,
        max: 100
      }
    });
  }
}
