import { Component, OnInit } from "@angular/core";

import { DashboardService } from "./dashboard.service";
import { Wallet } from "../../models/wallet";

@Component({
  selector: "app-dashboard",
  templateUrl: "dashboard.component.html"
})
export class DashboardComponent implements OnInit {
  public wallets: Wallet[];

  constructor(private service: DashboardService) {}

  ngOnInit() {
    this.service.getWallets().subscribe(wallets => this.wallets = wallets);
  }
}
