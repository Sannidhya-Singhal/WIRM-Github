export interface WorkItem {
  id: number;
  title: string;
  state: string;
  lastUpdate: Date;
  teamProject: string;
  link: string;
  favorite: Boolean;
}