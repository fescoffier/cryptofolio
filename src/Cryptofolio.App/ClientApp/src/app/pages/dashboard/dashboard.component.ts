import { Component, OnDestroy, OnInit } from "@angular/core";
import * as signalR from "@microsoft/signalr";

import { DashboardService } from "./dashboard.service";
import { Wallet } from "../../models/wallet";

@Component({
  selector: "app-dashboard",
  templateUrl: "dashboard.component.html"
})
export class DashboardComponent implements OnInit, OnDestroy {
  private connection: signalR.HubConnection;
  public wallets: Wallet[];

  constructor(private service: DashboardService) {}

  ngOnInit() {
    this.service.getWallets().subscribe(wallets => this.wallets = wallets);

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/dashboardHub")
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();
    this.connection.on("WalletBalanceChanged", (walletJson: any) => {
      const wallet = new Wallet(walletJson);
      this.wallets = this.wallets.map(w => w.id == wallet.id ? wallet : w);
    });
    this.connection
      .start()
      .then(() => {
        console.log(`SignalR connection succeeded! Connection id: ${this.connection.connectionId}`);
      })
      .catch((error) => {
        console.error(`SignalR connection start error: ${error}`);
      });
  }

  ngOnDestroy() {
    this.connection
      .stop()
      .then(() => {
        console.log("SignalR connection stopped!");
      })
      .catch((error) => {
        console.error(`SignalR connection stop error: ${error}`);
      });
  }
}
