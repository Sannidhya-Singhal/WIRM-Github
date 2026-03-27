import { AsyncPipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormsModule, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { KebabToWordsPipe } from '../../shared/pipes/kebab-case-to-words.pipe';
import { AutoExpandTextAreaDirective } from '../../shared/directives/auto-expand-text-area.directive';
import { BehaviorSubject, catchError, debounceTime, distinctUntilChanged, filter, of, switchMap, tap } from 'rxjs';
import { WorkItemsService } from '../../services/work-items.service';
import { createTicketForm } from '../../shared/types/ticket-form.type';

@Component({
  selector: 'app-ticket-form',
  standalone: true,
  templateUrl: './ticket-form.component.html',
  styleUrl: './ticket-form.component.css',
  imports: [
    AsyncPipe,
    FormsModule,
    ReactiveFormsModule,
    KebabToWordsPipe,
    AutoExpandTextAreaDirective,
  ]
})
export class TicketFormComponent implements OnInit, AfterViewInit {
  @ViewChild('inputStep1') inputStep1!: ElementRef<HTMLInputElement>;
  @ViewChild('inputStep2') inputStep2!: ElementRef<HTMLInputElement>;
  @ViewChild('inputStep3') inputStep3!: ElementRef<HTMLInputElement>;
  formBuilder = inject(NonNullableFormBuilder);
  ticketForm = createTicketForm(this.formBuilder);

  ticketTypes = [
    { id: 'new', 'name': 'Create a new application or feature', canFindReference: false, isReferenceRequired: false, isHidden: false },
    { id: 'update', 'name': 'Update the functionality of an existing application or feature', canFindReference: true, isReferenceRequired: false, isHidden: false },
    { id: 'bug-fix', 'name': 'Fix an error/bug occurring in an existing application or feature', canFindReference: true, isReferenceRequired: false, isHidden: false },
    // { id: 'new-script', canFindReference: false, isReferenceRequired: false }, 
    // { id: 'script-update', canFindReference: true, isReferenceRequired: true }, 
    // { id: 'script-bug-fix', canFindReference: true, isReferenceRequired: false }, 
    // { id: 'new-connector', canFindReference: false, isReferenceRequired: false }, 
  ];

  businessDriverTypes = [
    'Customer Retention',
    'Grow Revenue',
    'Cost Reduction/Savings',
    'New Revenue'
  ];

  revenueTypes = [
    { key: 'Less than $10K/month', label: '< $10k/month' },
    { key: 'Between $10K and $50K/month', label: '10k - 50k/month' },
    { key: 'Between $50K and $100K/month', label: '50k - 100k/month' },
    { key: 'Between $100K and $500K/month', label: '100k - 500k/month' },
    { key: 'Greater than $500K/month', label: '> 500k/month' }
  ];

  originalRequestorTypes = [
    'Lionbridge',
    'Customer'
  ];

  ProductTypes = [
    'Lionbridge Core Technology',
    'Custom Application, Tool, Solution or Script'
  ]

  subProductOptions = {
    'Lionbridge Core Technology': [
      'Aurora AI',
      'Aurora Array',
      'Aurora Studio',
      'Connectivity',
      'CPQ',
      'Content Remix App',
      'Content Remix Generator Template',
      'Gemini',
      'JTS',
      'LangAI System' ,
      'Language Cloud (LLC)',
      'LCX',
      'LTB',
      'ORT',
      'PowerBI and reporting',
      'QA App',
      'TMS',
      'TW',
      'Others']
  };

  verticalTypes = [
    'Enterprise',
    'Life Sciences (LSS)',
    'Legal',
    'Tech Vertical',
    'Transactional',
    'AIT',
    'Others',
  ];

  toolOrFeatureTypes = [
    'Convert',
    'Merge',
    'Macro',
  ];

  fileTypes = [
    'xlz',
    'xliff',
    'xlsx',
    'pdf',
    'json',
    'docx',
    'xml',
    'otm',
    'xlsm'
  ];

  // fileTypeListMapping: { [key: string]: string[] } = {
  //   Convert: ['xlz', 'xliff', 'xlsx', 'pdf', 'json', 'xml'],
  //   Merge: ['xlsx', 'docx', 'json', 'xml'],
  //   Macro: ['otm', 'xlsm'],
  //   Others: ['xlz', 'xliff', 'xlsx', 'pdf', 'json', 'docx', 'xml']
  // }
  //filterFileTypes: string[] = ['xlz', 'xliff', 'xlsx', 'pdf', 'json', 'xml', 'docx','otm', 'xlsm'];

  attachments: File[] = [];
  uploadValidationMessages: string[] = [];

  isTicketCreated$ = new BehaviorSubject<boolean>(false);
  isVerifying$ = new BehaviorSubject<boolean>(false);
  verificationError$ = new BehaviorSubject<string>('');
  isSubmittingForm$ = new BehaviorSubject<boolean>(false);
  submitFormMessage$ = new BehaviorSubject<string>('');
  submitFormErrorMessage$ = new BehaviorSubject<string>('');

  referenceWorkItemId: string | undefined;
  referenceWorkItemTeamProject: string | undefined;
  referenceWorkItemProductName: string | undefined;
  minDateTime: string = '';

  readonly maxFiles = 10;
  readonly maxSize = 60;
  readonly maxSizeBytes = this.maxSize * 1024 * 1024;
  readonly unsupportedFileTypes: string[] = [
    '.exe', '.bat',
  ];
  submitted: boolean = false;
  isBugFixEnabled: boolean = true;
  isOtherProductSelected: boolean = true;

  constructor(private workItemService: WorkItemsService) { }

  ngOnInit(): void {
    this.fileTypes.forEach(() =>
      this.ticketForm.controls.inputFileTypes
        .push(this.formBuilder.control(false))
    );
    this.setMinDateTime();
    this.setValueChangeSubscriptions();
    this.setValidations();

    const original =
      this.subProductOptions['Lionbridge Core Technology'];

    // Step 1: Sort alphabetically (Others last)
    const sorted = [...original].sort((a, b) => {
      if (a === 'Others') return 1;
      if (b === 'Others') return -1;
      return a.localeCompare(b, undefined, { sensitivity: 'base' });
    });

    // Step 2: Arrange vertically for 2-column grid
    this.subProductOptions['Lionbridge Core Technology'] =
      this.arrangeVertical(sorted, 2);
  }

  setMinDateTime() {
    const now = new Date();
    // Format to 'YYYY-MM-DDTHH:mm'
    const year = now.getFullYear();
    const month = this.padZero(now.getMonth() + 1);
    const day = this.padZero(now.getDate());
    const hours = this.padZero(now.getHours());
    const minutes = this.padZero(now.getMinutes());

    this.minDateTime = `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  padZero(num: number): string {
    return num < 10 ? '0' + num : num.toString();
  }

  setValueChangeSubscriptions(): void {
    this.ticketForm.controls.hasFirmDeadline.valueChanges
      .subscribe((isFirmDeadline: boolean) => {
        const deadlineCtrl = this.ticketForm.controls.deadline;
        isFirmDeadline ? deadlineCtrl?.enable() : deadlineCtrl?.disable();
      });

    this.ticketForm.controls.referenceWorkItemId.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        filter(value => typeof value === 'string' && value.length > 5),
        tap(() => {
          this.verificationError$.next('');
          this.isVerifying$.next(true);
        }),
        switchMap(value => {
          if (value !== undefined) {
            return this.workItemService.getWorkItemDetails(value)
              .pipe(catchError(err => {
                console.log(err);
                return of(null);
              })
              );
          }
          return of(null);
        }),
        tap(() => this.isVerifying$.next(false))
      ).subscribe(workItem => {
        if (workItem) {
          const controls = this.ticketForm.controls;

          controls.revenue.setValue(workItem.monthlyUSDRevenue);
          controls.customerName.setValue(workItem.customerName);
          controls.otherClientsAffected.setValue(workItem.otherClientsAffected);
          controls.businessDriver.setValue(workItem.businessDriver);
          if (workItem.reportingCustomer.includes('lionbridge'))
            controls.originalRequestorSide.setValue('Lionbridge');
          else
            controls.originalRequestorSide.setValue('Customer');


          this.mapVertical(workItem.regionVertical);
          this.referenceWorkItemId = workItem.id.toString();
          this.referenceWorkItemTeamProject = workItem.teamProject;
          this.referenceWorkItemProductName = workItem.productName;
          if (workItem.productName) {
            controls.toolOrFeatureType.setValue('Others');
            controls.otherToolOrFeatureType.setValue(workItem.productName);
          }
        } else {
          this.verificationError$.next('Invalid Ticket No.');
        }
      });

    // this.ticketForm.controls.toolOrFeatureType.valueChanges
    //   .subscribe(toolOrFeatureType => {
    //     this.filterFileTypes = this.fileTypeListMapping[toolOrFeatureType] || this.fileTypes;
    //   });

    this.ticketForm.get('productType')?.valueChanges.subscribe((selected) => {
      if (selected === 'Lionbridge Core Technology') {
        this.ticketForm.get('productDescription')?.reset();
        this.isOtherProductSelected = false;
        //this.ticketForm.controls.toolOrFeatureType.clearValidators();
        this.ticketForm.controls.productDescription.clearValidators();
        this.ticketForm.controls.subProductType.setValidators([Validators.required]);
        this.toggleBugFixOption(true);
      } else if (selected === 'Custom Application, Tool, Solution or Script') {
        this.ticketForm.get('subProductType')?.reset();
        this.isOtherProductSelected = true;
        this.isBugFixEnabled = true;
        this.toggleBugFixOption(false);
        this.ticketForm.controls.productDescription.setValidators([Validators.required]);
        // this.ticketForm.controls.toolOrFeatureType.setValidators([Validators.required]);
        this.ticketForm.controls.subProductType.clearValidators();
      }
      //this.ticketForm.controls.toolOrFeatureType.updateValueAndValidity();
      this.ticketForm.controls.productDescription.updateValueAndValidity();
      this.ticketForm.controls.subProductType.updateValueAndValidity();
    });
  }

  // Rearranges array to render column-wise in a row-based CSS grid
  private arrangeVertical(items: string[], columns: number): string[] {
    const rows = Math.ceil(items.length / columns);
    const result: string[] = [];

    for (let row = 0; row < rows; row++) {
      for (let col = 0; col < columns; col++) {
        const index = col * rows + row;
        if (index < items.length) {
          result.push(items[index]);
        }
      }
    }

    return result;
  }

  onEnterPress(event: KeyboardEvent | Event) {
    const target = event.target as HTMLElement;
    if (target.tagName.toLowerCase() !== 'textarea') {
      event.preventDefault();
    }
  }

  // Accessor for productType value
  get productTypeValue(): string {
    return this.ticketForm.controls['productType'].value;
  }

  get isCustomApp(): boolean {
    return this.productTypeValue === 'Custom Application, Tool, Solution or Script';
  }

  get isLionbridgeCore(): boolean {
    return this.productTypeValue === 'Lionbridge Core Technology';
  }

  get showProductTypeError(): boolean {
    return this.submitted && this.ticketForm.controls['productType'].invalid;
  }

  get showCustomAppDescriptionError(): boolean {
    return this.isCustomApp &&
      this.submitted &&
      this.ticketForm.controls['productType'].valid &&
      this.ticketForm.controls['productDescription'].invalid;
  }

  get showLionbridgeSubProductError(): boolean {
    return this.isLionbridgeCore &&
      this.submitted &&
      this.ticketForm.controls['productType'].valid &&
      this.ticketForm.controls['subProductType'].invalid;
  }

  get showLionbridgeOtherProductError(): boolean {
    return this.isLionbridgeCore &&
      this.submitted &&
      this.ticketForm.controls['subProductType'].valid &&
      this.ticketForm.controls['subProductTypeOther'].invalid;
  }

  toggleBugFixOption(show: boolean) {
    const bugType = this.ticketTypes.find(t => t.id === 'bug-fix');
    if (bugType) {
      bugType.isHidden = show;
    }
  }

  setValidations(): void {
    const controls = this.ticketForm.controls;
    controls.ticketType.setValidators([Validators.required]);
    controls.businessDriver.setValidators([Validators.required]);
    controls.revenue.setValidators([Validators.required]);
    controls.customerName.setValidators([Validators.required]);
    controls.verticalType.setValidators([Validators.required]);
    controls.originalRequestorSide.setValidators([Validators.required]);
    controls.productType.setValidators([Validators.required]);
    controls.requestEmailAddress.setValidators([Validators.required, Validators.email]);
    controls.titlePart.setValidators([Validators.required]);
    controls.ticketDescription.setValidators([Validators.required]);

    controls.requestEmailAddress.updateValueAndValidity();

    controls.ticketType.valueChanges
      .subscribe(value => {
        const ticketType = this.ticketTypes.find(i => i.id === value);
        const referenceTicketNoCtrl = controls.referenceWorkItemId;

        this.clearValidators(referenceTicketNoCtrl);
        this.referenceWorkItemId = undefined;
        this.referenceWorkItemProductName = undefined;
        this.referenceWorkItemTeamProject = undefined;
        this.verificationError$.next('');
        referenceTicketNoCtrl?.updateValueAndValidity();
      });

    controls.hasFirmDeadline.valueChanges
      .subscribe(value => {
        const deadlineCtrl = controls.deadline;
        // const justificationCtrl = controls.deadlineJustification;
        if (value) {
          deadlineCtrl?.setValidators([Validators.required]);
          //justificationCtrl?.setValidators([Validators.required]);
        } else {
          this.clearValidators(deadlineCtrl, null);
          //this.clearValidators(justificationCtrl);
        }
        //justificationCtrl?.updateValueAndValidity();
        deadlineCtrl?.updateValueAndValidity();
      });

    controls.verticalType.valueChanges
      .subscribe(value => {
        const otherControl = controls.otherVerticalType;
        if (value === 'Others') {
          otherControl.setValidators([Validators.required]);
        } else {
          this.clearValidators(otherControl);
        }
        otherControl?.updateValueAndValidity();
      });

    controls.subProductType.valueChanges
      .subscribe(value => {
        const otherControl = controls.subProductTypeOther;
        if (value === 'Others') {
          otherControl.setValidators([Validators.required]);
        } else {
          this.clearValidators(otherControl);
        }
        otherControl?.updateValueAndValidity();
      });

    // controls.toolOrFeatureType.valueChanges
    //   .subscribe(value => {
    //     const otherControl = controls.otherToolOrFeatureType;
    //     if (value === 'Others') {
    //       otherControl.setValidators([Validators.required]);
    //     } else {
    //       this.clearValidators(otherControl);
    //     }
    //     otherControl?.updateValueAndValidity();
    //   });      

    // controls.hasOtherInputFileType.valueChanges
    //   .subscribe(value => {
    //     const otherControl = controls.otherInputFileType;
    //     if (value) {
    //       otherControl.setValidators([Validators.required]);
    //     } else {
    //       this.clearValidators(otherControl);
    //     }
    //     otherControl?.updateValueAndValidity();
    //   });
  }

  ngAfterViewInit(): void {
    // this.ticketForm.controls.ticketType.valueChanges.subscribe((value) => {
    //   if (value === 'new') {

    //     this.inputStep2.nativeElement.checked = true;

    //   }
    // })
  }

  mapVertical(regionVertical: string): void {
    let verticalType = this.verticalTypes.find(i => i === regionVertical);
    if (!verticalType) {
      verticalType = 'Others'
      this.ticketForm.controls.otherVerticalType.setValue(regionVertical);
    }
    this.ticketForm.controls.verticalType.setValue(verticalType);
  }

  clearValidators(control: FormControl, emptyVal: any = ''): void {
    control?.clearValidators();
    control?.setValue(emptyVal);
    control?.setErrors(null);
  }

  onFilesSelected(event: Event) {
    this.uploadValidationMessages = [];
    const input = event.target as HTMLInputElement;
    const files = input.files;
    if (!files) return;

    const newFiles = Array.from(files);
    for (const file of newFiles) {
      if (this.attachments.find(i => i.name === file.name)) continue;
      if (this.attachments.length >= this.maxFiles) {
        const index = newFiles.indexOf(file);
        this.uploadValidationMessages.push(`Unable to add the following file(s) as max limit of attachment is reached:`);
        const unattachedFiles = newFiles.slice(index);
        for (const file of unattachedFiles) {
          this.uploadValidationMessages.push(file.name);
        }
        break;
      }

      const ext = this.getFileExtension(file);
      if (ext && this.unsupportedFileTypes.includes(ext)) {
        this.uploadValidationMessages.push(`File Type of ${file.name} is not supported.`);
        continue;
      }
      if (file.size > this.maxSizeBytes) {
        this.uploadValidationMessages.push(`File ${file.name} (${this.getFileSize(file)} MB) exceeds file size limit of ${this.maxSize} MB.`);
        continue;
      }

      this.attachments.push(file);
    }

    input.value = '';
  }

  getFileName(file: File): string | undefined {
    if (!file) return undefined;
    const index = file.name.indexOf('.');
    return index === -1 ? file.name : file.name.slice(0, index);
  }
  getFileExtension(file: File): string | undefined {
    if (!file) return undefined;
    if (!file.name.includes('.')) return undefined;
    return file.name.split('.').pop()?.toLowerCase();
  }
  getFileSize(file: File): number {
    if (!file) return 0;
    const mbBytes = 1024 * 1024;
    return +(file.size / mbBytes).toFixed(2);
  }

  removeFile(index: number): void {
    this.attachments.splice(index, 1);
  }

  onSubmit(): void {

    this.submitted = true;
    if (this.ticketForm.invalid) {
      this.ticketForm.markAllAsTouched();
    }

    this.submitFormErrorMessage$.next('');
    this.submitFormMessage$.next('');

    const controls = this.ticketForm.controls;
    const stepValidations = [
      {
        step: this.inputStep1,
        controls: [controls.ticketType, controls.productType, controls.productDescription, controls.subProductTypeOther]
      },
      {
        step: this.inputStep2,
        controls: [controls.businessDriver, controls.deadline, controls.revenue]
      },
      {
        step: this.inputStep3,
        controls: [controls.customerName, controls.originalRequestorSide, controls.verticalType, controls.otherVerticalType, controls.requestEmailAddress]
      }
    ]

    if (this.ticketForm.valid) {
      this.isSubmittingForm$.next(true);

      const ticketType = this.ticketForm.controls.ticketType.value;
      const isNewToolOrScript = ticketType === 'new';

      let productName = isNewToolOrScript
        ? this.ticketForm.controls.otherToolOrFeatureType.value
        : this.referenceWorkItemProductName;

      const rawData = {
        ...this.ticketForm.getRawValue(),
        referenceWorkItemProductName: productName,
        referenceWorkItemTeamProject: this.referenceWorkItemTeamProject,
      };

      let inputFileFormats: string[] = this.fileTypes.filter((_, i) => rawData.inputFileTypes[i]);
      if (rawData.hasOtherInputFileType && rawData.otherInputFileType.trim().length > 0) {
        const otherTypes = rawData.otherInputFileType
          .split(',')
          .map(type => type.trim())
          .filter(type => type.length > 0);
        inputFileFormats = [...inputFileFormats, ...otherTypes];
      }
      rawData.inputFileFormats = inputFileFormats;

      const payload = new FormData();
      payload.append('form', JSON.stringify(rawData));
      this.attachments.forEach(file => payload.append('files', file, file.name));

      this.workItemService.createTicket(payload)
        .subscribe({
          next: (workItem) => {
            this.isSubmittingForm$.next(false);
            let message = `Ticket has been successfully created. Your new ticket no is ${workItem.id}. Weighting: ${workItem.weighting}`;
            message = message.replace(
              /Your new ticket no is (\d+)/,
              `Your new ticket no is <a class="underline underline-offset-4" href='${workItem.link}' target="_blank" rel="noopener noreferrer">$1</a>`
            );
            this.submitFormMessage$.next(message);
            this.isTicketCreated$.next(true);
          }, error: (error) => {
            console.log(error);
            this.isSubmittingForm$.next(false);
            this.submitFormErrorMessage$.next(error.error);
          }
        });
    } else {
      for (const controlGroup of stepValidations) {
        const hasError = controlGroup.controls.some(c => c.errors);
        if (hasError) {
          controlGroup.step.nativeElement.checked = true;
          break;
        }
      }
    }
  }
}
