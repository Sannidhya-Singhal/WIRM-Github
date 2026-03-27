import { FormArray, FormControl, FormGroup, NonNullableFormBuilder } from "@angular/forms";

export type TicketForm = {
    ticketType: FormControl<string>;
    referenceWorkItemId: FormControl<string|undefined>;
    productionBlock: FormControl<boolean>,
    hasWorkaround: FormControl<boolean>,
    businessDriver: FormControl<string>,
    businessCategory:FormControl<string>,
    hasFirmDeadline: FormControl<boolean>;
    deadline: FormControl<Date | null>;
    deadlineJustification: FormControl<string>;
    revenue: FormControl<string>;
    customerName: FormControl<string>;
    requestEmailAddress : FormControl<string>;
    otherClientsAffected: FormControl<string>;
    originalRequestorSide: FormControl<string>;
    productType:FormControl<string>;
    subProductType:FormControl<string>,
    productDescription:FormControl<string>,
    subProductTypeOther:FormControl<string>
    verticalType: FormControl<string>;
    otherVerticalType: FormControl<string>;
    isToolRequest: FormControl<boolean>
    toolOrFeatureType: FormControl<string>;
    otherToolOrFeatureType: FormControl<string>;
    inputFileTypes: FormArray<FormControl<boolean>>;
    hasOtherInputFileType: FormControl<boolean>;    
    otherInputFileType: FormControl<string>;
    inputFileFormats: FormControl<string[]>;
    geminiNumber: FormControl<string>;
    titlePart: FormControl<string>;
    ticketDescription: FormControl<string>;
    acceptanceCriteria: FormControl<string>; 
}

export function createTicketForm(fb: NonNullableFormBuilder): FormGroup<TicketForm> {
    const emptyControls = [] as FormControl<boolean>[];
    return fb.group<TicketForm>({
        ticketType: fb.control(''),
        referenceWorkItemId: fb.control(undefined),
        productionBlock: fb.control(false),
        hasWorkaround: fb.control(false),
        businessDriver: fb.control(''),
        businessCategory:fb.control(''),
        hasFirmDeadline: fb.control(false),
        deadline: fb.control({ value: null, disabled: true }),
        deadlineJustification: fb.control(''),
        revenue: fb.control(''),
        customerName: fb.control(''),
        requestEmailAddress:fb.control(''),
        otherClientsAffected: fb.control(''),
        originalRequestorSide: fb.control(''),
        productType:fb.control(''),
        subProductType:fb.control(''),
        subProductTypeOther:fb.control(''),
        productDescription:fb.control(''),
        verticalType: fb.control(''),
        otherVerticalType: fb.control(''),
        isToolRequest: fb.control(true),
        toolOrFeatureType: fb.control(''),
        otherToolOrFeatureType: fb.control(''),
        inputFileTypes: fb.array(emptyControls),
        hasOtherInputFileType: fb.control(false),
        otherInputFileType: fb.control(''),
        inputFileFormats: fb.control([]),
        geminiNumber: fb.control(''),
        titlePart: fb.control(''),
        ticketDescription: fb.control(''),
        acceptanceCriteria: fb.control('')
    });
} 

