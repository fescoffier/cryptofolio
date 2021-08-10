import { Component, OnInit } from "@angular/core";

import * as mapsData from "devextreme/dist/js/vectormap-data/world.js";
import { Countries, Service } from "./vector-map.service";

@Component({
  selector: "app-vector",
  providers: [Service],
  templateUrl: "vector.component.html"
})
export class VectorComponent implements OnInit {
  worldMap: any = mapsData.world;
  countries: Countries;

  constructor(service: Service) {
    this.countries = service.getCountries();
    this.customizeTooltip = this.customizeTooltip.bind(this);
    this.customizeLayers = this.customizeLayers.bind(this);
    this.click = this.click.bind(this);
  }

  customizeTooltip(arg) {
    let name = arg.attribute("name");
    return {
      text: name,
      color: "#FFFFFF",
      fontColor: "#000"
    };
  }

  customizeLayers(elements) {
    elements.forEach(element => {
      let country = this.countries[element.attribute("name")];
      if (country) {
        element.applySettings({
          color: country.color,
          hoveredColor: country.color,
          selectedColor: country.color
        });
      } else {
        element.applySettings({
          color: "#e4e4e4",
          hoveredColor: "#e4e4e4",
          selectedColor: "#e4e4e4"
        });
      }
    });
  }

  click(e) {
    let target = e.target;
    if (target && this.countries[target.attribute("name")]) {
      target.selected(!target.selected());
    }
  }
  ngOnInit() {}
}
