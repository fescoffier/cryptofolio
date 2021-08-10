import { Component, OnInit, OnDestroy } from "@angular/core";

@Component({
  selector: "app-pricing",
  templateUrl: "pricing.component.html"
})
export class PricingComponent implements OnInit, OnDestroy {
  constructor() {}

  ngOnInit() {
    var body = document.getElementsByTagName("body")[0];
    body.classList.add("pricing-page");
  }
  ngOnDestroy() {
    var body = document.getElementsByTagName("body")[0];
    body.classList.remove("pricing-page");
  }
}
