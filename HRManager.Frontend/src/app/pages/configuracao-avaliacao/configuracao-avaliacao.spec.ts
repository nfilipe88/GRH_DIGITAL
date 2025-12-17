import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfiguracaoAvaliacao } from './configuracao-avaliacao';

describe('ConfiguracaoAvaliacao', () => {
  let component: ConfiguracaoAvaliacao;
  let fixture: ComponentFixture<ConfiguracaoAvaliacao>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfiguracaoAvaliacao]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfiguracaoAvaliacao);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
