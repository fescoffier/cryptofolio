import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app-panels",
  templateUrl: "panels.component.html"
})
export class PanelsComponent implements OnInit {
  collapseOne = true;
  collapseThree = true;
  collapseTwo = true;
  constructor() {}

  ngOnInit() {}
}
